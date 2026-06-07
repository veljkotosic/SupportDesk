using Microsoft.EntityFrameworkCore;
using SupportDeskWebApi.Data.Database;
using SupportDeskWebApi.Data.Entities.Ticket.Enums;
using SupportDeskWebApi.Data.Entities.TicketNotification.Enums;
using SupportDeskWebApi.Requests.Abstract;
using SupportDeskWebApi.Requests.Ticket.Common;
using SupportDeskWebApi.Requests.TicketNotification.Common;

namespace SupportDeskWebApi.Requests.Ticket.GetCustomerTickets;

public class GetCustomerTicketsRequestHandler
    : IRequestHandler<GetCustomerTicketsRequest, GetCustomerTicketsResult>
{
    private readonly SupportDeskDbContext _context;

    public GetCustomerTicketsRequestHandler(SupportDeskDbContext context)
    {
        _context = context;
    }

    public async Task<GetCustomerTicketsResult> HandleAsync(GetCustomerTicketsRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Skip < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request.Skip), "Skip cannot be negative.");
        }

        if (request.Take is < 1 or > 50)
        {
            throw new ArgumentOutOfRangeException(nameof(request.Take), "Take must be between 1 and 50.");
        }

        var ticketsQuery = _context.Tickets.AsNoTracking();

        var totalCount = await ticketsQuery.CountAsync(cancellationToken);
        var openCount = await ticketsQuery.CountAsync(t => t.Status == TicketStatus.Open, cancellationToken);
        var assignedCount = await ticketsQuery.CountAsync(t => t.Status == TicketStatus.Assigned, cancellationToken);
        var closedCount = await ticketsQuery.CountAsync(t => t.Status == TicketStatus.Closed, cancellationToken);

        var tickets = await ticketsQuery
            .OrderByDescending(t => t.LastMessageAt)
            .ThenByDescending(t => t.Id)
            .Skip(request.Skip)
            .Take(request.Take)
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

        return new GetCustomerTicketsResult(tickets, totalCount, openCount, assignedCount, closedCount);
    }
}
