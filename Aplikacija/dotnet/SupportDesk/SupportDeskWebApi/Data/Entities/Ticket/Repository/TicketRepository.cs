using Microsoft.EntityFrameworkCore;
using SupportDeskWebApi.Data.Database;
using SupportDeskWebApi.Data.Entities.Common.Repository;
using SupportDeskWebApi.Data.Entities.TicketNotification.Enums;
using SupportDeskWebApi.Requests.Message.Common;
using SupportDeskWebApi.Requests.Note.Common;
using SupportDeskWebApi.Requests.Ticket.Common;
using SupportDeskWebApi.Requests.Ticket.GetTicketViewInfo;
using SupportDeskWebApi.Requests.TicketNotification.Common;

namespace SupportDeskWebApi.Data.Entities.Ticket.Repository;

public class TicketRepository : Repository<Ticket>, ITicketRepository
{
    public TicketRepository(SupportDeskDbContext context) 
        : base(context)
    {
        
    }

    public async Task<List<TicketDetailsDto>> GetCustomerTicketsAsync(CancellationToken cancellationToken = default)
    {
        return await GetTicketDetailsQuery().IgnoreQueryFilters().ToListAsync(cancellationToken);
    }

    public async Task<TicketDetailsDto?> GetTicketAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await GetTicketDetailsQuery(id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TicketViewInfoDto?> GetTicketViewInfoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context.Tickets
            .Where(t => t.Id == id)
            .Select(t => new TicketViewInfoDto(
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
                t.Messages.Select(m => new MessageDetailsDto(
                    m.Id,
                    m.SenderId,
                    m.Sender.UserName!,
                    m.Text,
                    m.CreatedAt)).ToList(),
                t.Notes.Select(n => new NoteDetailsDto(
                    n.Id,
                    n.AuthorId,
                    n.Author.UserName!,
                    n.Text,
                    n.CreatedAt)).ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private IQueryable<TicketDetailsDto> GetTicketDetailsQuery(Guid id)
    {
        return Context.Tickets
            .Where(t => t.Id == id)
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
                t.Notifications
                    .Where(n => n.Status == TicketNotificationStatus.Unread)
                    .Select(n => new TicketNotificationDetailsDto(
                    n.Id,
                    n.OrganizationId,
                    n.TicketId,
                    n.Text,
                    n.Status,
                    n.CreatedAt))
                    .ToList()
            ));
    }
    
    private IQueryable<TicketDetailsDto> GetTicketDetailsQuery()
    {
        return Context.Tickets
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
                t.Notifications
                    .Where(n => n.Status == TicketNotificationStatus.Unread)
                    .Select(n => new TicketNotificationDetailsDto(
                        n.Id,
                        n.OrganizationId,
                        n.TicketId,
                        n.Text,
                        n.Status,
                        n.CreatedAt))
                    .ToList()
            ));
    }
}