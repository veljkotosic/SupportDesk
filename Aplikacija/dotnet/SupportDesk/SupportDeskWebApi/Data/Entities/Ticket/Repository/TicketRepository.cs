using SupportDeskWebApi.Data.Database;
using SupportDeskWebApi.Data.Entities.Common.Repository;

namespace SupportDeskWebApi.Data.Entities.Ticket.Repository;

public class TicketRepository : Repository<Ticket>, ITicketRepository
{
    public TicketRepository(SupportDeskDbContext context) 
        : base(context)
    {
        
    }
}
