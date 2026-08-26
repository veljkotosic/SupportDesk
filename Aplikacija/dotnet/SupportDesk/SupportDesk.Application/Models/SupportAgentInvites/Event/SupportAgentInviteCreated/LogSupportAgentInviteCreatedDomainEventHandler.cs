using Microsoft.Extensions.Logging;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Models.SupportAgentInvite.Events;

namespace SupportDesk.Application.Models.SupportAgentInvites.Event.SupportAgentInviteCreated;

public sealed class LogSupportAgentInviteCreatedDomainEventHandler
    : IDomainEventHandler<SupportAgentInviteCreatedDomainEvent>
{
    private readonly ILogger<LogSupportAgentInviteCreatedDomainEventHandler> _logger;

    public LogSupportAgentInviteCreatedDomainEventHandler(ILogger<LogSupportAgentInviteCreatedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(SupportAgentInviteCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Support agent invite with id '{SupportAgentInviteId}' created.", domainEvent.SupportAgentInviteCodeId.IdValue);
        
        return Task.CompletedTask;
    }
}