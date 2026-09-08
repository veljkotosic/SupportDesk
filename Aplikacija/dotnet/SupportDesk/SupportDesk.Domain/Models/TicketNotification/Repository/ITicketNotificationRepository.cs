using SupportDesk.Domain.Abstract.Repository;
using SupportDesk.Domain.Models.Ticket.ValueObjects;
using SupportDesk.Domain.Models.TicketNotification.ValueObjects;

namespace SupportDesk.Domain.Models.TicketNotification.Repository;

public interface ITicketNotificationRepository : IAbstractRepository<TicketNotification, TicketNotificationId>
{
    Task<ICollection<TicketNotification>> GetUnreadNotificationsByTicketIdAsync(TicketId ticketId, CancellationToken cancellationToken = default);
}