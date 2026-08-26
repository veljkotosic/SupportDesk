using Microsoft.Extensions.Logging;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Models.SupportAgentInvite.Events;

namespace SupportDesk.Application.Models.SupportAgentInvites.Event.SupportAgentInviteUsed;

public sealed class LogSupportAgentInviteUsedDomainEventHandler
    : IDomainEventHandler<SupportAgentInviteUsedDomainEvent>
{
    private readonly ILogger<LogSupportAgentInviteUsedDomainEventHandler> _logger;

    public LogSupportAgentInviteUsedDomainEventHandler(ILogger<LogSupportAgentInviteUsedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(SupportAgentInviteUsedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Support agent invite with id '{Id}' has been used.", domainEvent.SupportAgentInviteId.IdValue);
        
        return Task.CompletedTask;
    }
}