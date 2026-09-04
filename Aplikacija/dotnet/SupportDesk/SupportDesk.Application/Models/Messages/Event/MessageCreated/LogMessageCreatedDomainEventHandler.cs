using Microsoft.Extensions.Logging;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Models.Message.Events;

namespace SupportDesk.Application.Models.Messages.Event.MessageCreated;

internal sealed class LogMessageCreatedDomainEventHandler
    : IDomainEventHandler<MessageCreatedDomainEvent>
{
    private readonly ILogger<LogMessageCreatedDomainEventHandler> _logger;

    public LogMessageCreatedDomainEventHandler(ILogger<LogMessageCreatedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(MessageCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Message with id '{MessageId}' created.", domainEvent.MessageId.IdValue);
        
        return Task.CompletedTask;
    }
}