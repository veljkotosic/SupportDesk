using Microsoft.EntityFrameworkCore;
using SupportDeskWebApi.Data.Database;
using SupportDeskWebApi.Data.Entities.TicketNotification.Enums;
using SupportDeskWebApi.Requests.Abstract;
using SupportDeskWebApi.Requests.Ticket.Common;
using SupportDeskWebApi.Requests.TicketNotification.Common;

namespace SupportDeskWebApi.Requests.Ticket.GetTicket;

public class GetTicketRequestHandler
    : IRequestHandler<GetTicketRequest, GetTicketResult>
{
    private readonly SupportDeskDbContext _context;

    public GetTicketRequestHandler(SupportDeskDbContext context)
    {
        _context = context;
    }

    public async Task<GetTicketResult> HandleAsync(GetTicketRequest request, CancellationToken cancellationToken = default)
    {
        var ticket = await _context.Tickets
            .AsNoTracking()
            .Where(t => t.Id == request.TicketId)
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
            .FirstOrDefaultAsync(cancellationToken);

        if (ticket is null)
        {
            throw new Exception("Ticket not found.");       
        }

        return new GetTicketResult(ticket);
    }
}
