using Microsoft.EntityFrameworkCore;
using SupportDeskWebApi.Data.Database;
using SupportDeskWebApi.Data.Entities.Common.Repository;
using SupportDeskWebApi.Requests.Ticket.Common;

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
                t.Feedback
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
                t.Feedback
            ));
    }
}