using Microsoft.Extensions.Logging;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Models.Ticket.Events;

namespace SupportDesk.Application.Models.Tickets.Event.TicketFeedbackGiven;

internal sealed class LogTicketFeedbackGivenDomainEventHandler
    : IDomainEventHandler<TicketFeedbackGivenDomainEvent>
{
    private readonly ILogger<LogTicketFeedbackGivenDomainEventHandler> _logger;

    public LogTicketFeedbackGivenDomainEventHandler(ILogger<LogTicketFeedbackGivenDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(TicketFeedbackGivenDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Ticket with id '{TicketId}' has been given a feedback.", domainEvent.TicketId.IdValue);
        
        return Task.CompletedTask;      
    }
}