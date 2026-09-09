using SupportDesk.Domain.Abstract.Repository;
using SupportDesk.Domain.Models.Ticket.ValueObjects;

namespace SupportDesk.Domain.Models.Ticket.Repository;

public interface ITicketRepository : IAbstractRepository<Ticket, TicketId>
{
    
}