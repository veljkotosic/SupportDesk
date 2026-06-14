using Microsoft.EntityFrameworkCore;
using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Database;
using SupportDeskWebApi.Data.Entities.Ticket.Enums;
using SupportDeskWebApi.Data.Entities.TicketNotification.Enums;
using SupportDeskWebApi.Data.Entities.User;
using SupportDeskWebApi.Requests.Abstract;
using SupportDeskWebApi.Requests.Ticket.Common;
using SupportDeskWebApi.Requests.TicketNotification.Common;

namespace SupportDeskWebApi.Requests.OrganizationAdmin.GetDashboard;

public class GetDashboardRequestHandler
    : IRequestHandler<GetDashboardRequest, GetDashboardResult>
{
    private readonly SupportDeskDbContext _context;
    private readonly IUserContext _userContext;

    public GetDashboardRequestHandler(
        SupportDeskDbContext context,
        IUserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    public async Task<GetDashboardResult> HandleAsync(
        GetDashboardRequest request,
        CancellationToken cancellationToken = default)
    {
        var organizationId = _userContext.GetCurrentUsersOrganizationId()!;

        var organizationName = await _context.Organizations
            .Where(organization => organization.Id == organizationId)
            .Select(organization => organization.Name)
            .FirstAsync(cancellationToken);

        var supportAgentsQuery = _context.Users
            .Where(user => user.OrganizationId == organizationId && user.Type == UserType.SupportAgent);

        var openTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Open, cancellationToken);
        var assignedTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Assigned, cancellationToken);
        var closedTickets = await _context.Tickets.CountAsync(t => t.Status == TicketStatus.Closed, cancellationToken);
        var agentCount = await supportAgentsQuery.CountAsync(cancellationToken);
        
        var summary = new DashboardSummaryDto(openTickets, assignedTickets, closedTickets, agentCount);

        var agents = await supportAgentsQuery
            .AsNoTracking()
            .OrderBy(u => u.UserName)
            .Select(u => new DashboardAgentDto(
                u.Id,
                u.UserName!,
                u.AssignedTickets.Count(t => t.Status == TicketStatus.Assigned)))
            .ToListAsync(cancellationToken);

        var recentTickets = await _context.Tickets
            .AsNoTracking()
            .OrderByDescending(t => t.LastMessageAt)
            .Take(6)
            .Select(t => new TicketDetailsDto(
                t.Id,
                t.OrganizationId,
                t.Organization.Name,
                t.CategoryId,
                t.Category.Name,
                t.CustomerId,
                t.Customer.UserName!,
                t.SupportAgentId,
                t.SupportAgent != null ? t.SupportAgent.UserName : null,
                t.Status,
                t.Priority,
                t.Subject,
                t.OpenedAt,
                t.AssignedAt,
                t.ClosedAt,
                t.Feedback,
                t.LastMessageAt,
                t.Notifications
                    .Where(n => n.Status == TicketNotificationStatus.Unread)
                    .Select(n => new TicketNotificationDetailsDto(
                        n.Id,
                        n.OrganizationId,
                        n.TicketId,
                        n.Text,
                        n.Status,
                        n.CreatedAt))
                    .ToList()))
            .ToListAsync(cancellationToken);

        var startDate = DateTime.UtcNow.Date.AddDays(-6);
        var volumeTickets = await _context.Tickets
            .AsNoTracking()
            .Where(t => t.OpenedAt >= startDate || t.ClosedAt >= startDate)
            .Select(t => new { t.OpenedAt, t.ClosedAt, t.Status })
            .ToListAsync(cancellationToken);

        var ticketVolume = Enumerable.Range(0, 7)
            .Select(offset =>
            {
                var date = startDate.AddDays(offset);
                return new TicketVolumeEntryDto(
                    DateOnly.FromDateTime(date),
                    volumeTickets.Count(t => t.OpenedAt.Date == date),
                    volumeTickets.Count(t => t.Status == TicketStatus.Closed && t.ClosedAt.Date == date));
            })
            .ToList();

        return new GetDashboardResult(organizationName, summary, ticketVolume, agents, recentTickets);
    }
}
