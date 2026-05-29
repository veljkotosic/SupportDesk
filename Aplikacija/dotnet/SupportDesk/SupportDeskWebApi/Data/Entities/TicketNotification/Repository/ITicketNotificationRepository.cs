using SupportDeskWebApi.Data.Entities.Common.Repository;

namespace SupportDeskWebApi.Data.Entities.TicketNotification.Repository;

public interface ITicketNotificationRepository : IRepository<TicketNotification>
{
    Task<List<TicketNotification>> GetUnreadNotificationsAsync(Guid ticketId, CancellationToken cancellationToken = default);
}