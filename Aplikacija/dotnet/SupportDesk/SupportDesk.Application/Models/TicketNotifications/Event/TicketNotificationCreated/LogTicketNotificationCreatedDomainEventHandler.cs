using Microsoft.Extensions.Logging;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Models.TicketNotification.Events;

namespace SupportDesk.Application.Models.TicketNotifications.Event.TicketNotificationCreated;

internal sealed class LogTicketNotificationCreatedDomainEventHandler
    : IDomainEventHandler<TicketNotificationCreatedDomainEvent>
{
    private readonly ILogger<LogTicketNotificationCreatedDomainEventHandler> _logger;

    public LogTicketNotificationCreatedDomainEventHandler(ILogger<LogTicketNotificationCreatedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(TicketNotificationCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Ticket notification with id '{TicketNotificationId}' created.", domainEvent.TicketNotificationId.IdValue);
        
        return Task.CompletedTask;       
    }
}