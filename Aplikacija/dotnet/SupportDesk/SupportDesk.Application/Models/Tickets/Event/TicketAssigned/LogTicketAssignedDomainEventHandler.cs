using Microsoft.Extensions.Logging;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Models.Ticket.Events;

namespace SupportDesk.Application.Models.Tickets.Event.TicketAssigned;

internal sealed class LogTicketAssignedDomainEventHandler
    : IDomainEventHandler<TicketAssignedDomainEvent>
{
    private readonly ILogger<LogTicketAssignedDomainEventHandler> _logger;

    public LogTicketAssignedDomainEventHandler(ILogger<LogTicketAssignedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(TicketAssignedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Ticket with id '{TicketId}' assigned to agent with id '{SupportAgentId}'.",
            domainEvent.TicketId.IdValue,
            domainEvent.SupportAgentId.IdValue);
        
        return Task.CompletedTask;      
    }
}