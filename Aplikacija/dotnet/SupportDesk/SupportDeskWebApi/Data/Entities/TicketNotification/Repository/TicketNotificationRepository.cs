using Microsoft.EntityFrameworkCore;
using SupportDeskWebApi.Data.Database;
using SupportDeskWebApi.Data.Entities.Common.Repository;
using SupportDeskWebApi.Data.Entities.TicketNotification.Enums;

namespace SupportDeskWebApi.Data.Entities.TicketNotification.Repository;

public class TicketNotificationRepository
    : Repository<TicketNotification>, ITicketNotificationRepository
{
    public TicketNotificationRepository(SupportDeskDbContext context)
        : base(context)
    {
        
    }

    public async Task<List<TicketNotification>> GetUnreadNotificationsAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        return await Context.TicketNotifications
            .Where(tn => tn.TicketId == ticketId && tn.Status == TicketNotificationStatus.Unread)
            .ToListAsync(cancellationToken);
    }
}