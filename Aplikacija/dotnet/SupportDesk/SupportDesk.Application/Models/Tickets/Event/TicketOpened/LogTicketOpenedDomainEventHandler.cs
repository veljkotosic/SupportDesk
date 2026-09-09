using Microsoft.Extensions.Logging;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Models.Ticket.Events;

namespace SupportDesk.Application.Models.Tickets.Event.TicketOpened;

internal sealed class LogTicketOpenedDomainEventHandler
    : IDomainEventHandler<TicketOpenedDomainEvent>
{
    private readonly ILogger<LogTicketOpenedDomainEventHandler> _logger;

    public LogTicketOpenedDomainEventHandler(ILogger<LogTicketOpenedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(TicketOpenedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Ticket with id '{TicketId}' opened.'", domainEvent.TicketId.IdValue);
        
        return Task.CompletedTask;       
    }
}