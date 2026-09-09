using Microsoft.Extensions.Logging;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Models.Ticket.Events;

namespace SupportDesk.Application.Models.Tickets.Event.TicketClosed;

internal sealed class LogTicketClosedDomainEventHandler
    : IDomainEventHandler<TicketClosedDomainEvent>
{
    private readonly ILogger<LogTicketClosedDomainEventHandler> _logger;

    public LogTicketClosedDomainEventHandler(ILogger<LogTicketClosedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(TicketClosedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Ticket with id '{TicketId}' has been closed.", domainEvent.TicketId.IdValue);
        
        return Task.CompletedTask;      
    }
}