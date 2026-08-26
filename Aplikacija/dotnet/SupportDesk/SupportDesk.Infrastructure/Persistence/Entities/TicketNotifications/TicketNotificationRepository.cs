using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Models.TicketNotification;
using SupportDesk.Domain.Models.TicketNotification.Repository;
using SupportDesk.Domain.Models.TicketNotification.ValueObjects;
using SupportDesk.Infrastructure.Persistence.Abstract;
using SupportDesk.Infrastructure.Persistence.Database;

namespace SupportDesk.Infrastructure.Persistence.Entities.TicketNotifications;

public sealed class TicketNotificationRepository
    : AbstractRepository<TicketNotification, TicketNotificationId>, ITicketNotificationRepository
{
    public TicketNotificationRepository(
        SupportDeskDbContext context,
        IDomainEventCollector domainEventCollector) 
        : base(context, domainEventCollector)
    {
        
    }
}