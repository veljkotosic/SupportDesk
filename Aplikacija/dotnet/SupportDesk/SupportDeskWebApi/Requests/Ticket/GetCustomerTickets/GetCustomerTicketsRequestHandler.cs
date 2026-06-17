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
        var customerTickets = _context.Tickets.AsNoTracking();

        var allCount = await customerTickets.CountAsync(cancellationToken);
        var openCount = await customerTickets.CountAsync(t => t.Status == TicketStatus.Open, cancellationToken);
        var assignedCount = await customerTickets.CountAsync(t => t.Status == TicketStatus.Assigned, cancellationToken);
        var closedCount = await customerTickets.CountAsync(t => t.Status == TicketStatus.Closed, cancellationToken);

        var filteredTickets = customerTickets;
        var search = request.Search?.Trim();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchPattern = $"%{search}%";
            var parsedTicketId = Guid.TryParse(search, out var ticketId) ? ticketId : (Guid?)null;

            filteredTickets = filteredTickets.Where(ticket =>
                (parsedTicketId.HasValue && ticket.Id == parsedTicketId.Value) ||
                EF.Functions.ILike(ticket.Subject, searchPattern) ||
                EF.Functions.ILike(ticket.Organization.Name, searchPattern) ||
                EF.Functions.ILike(ticket.Category.Name, searchPattern));
        }

        if (request.Status.HasValue)
        {
            filteredTickets = filteredTickets.Where(ticket => ticket.Status == request.Status.Value);
        }

        var totalCount = await filteredTickets.CountAsync(cancellationToken);

        var tickets = await filteredTickets
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

        return new GetCustomerTicketsResult(tickets, totalCount, allCount, openCount, assignedCount, closedCount);
    }
}
