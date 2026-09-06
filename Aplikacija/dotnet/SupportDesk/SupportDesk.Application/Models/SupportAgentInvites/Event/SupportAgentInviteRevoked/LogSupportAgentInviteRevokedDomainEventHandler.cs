using Microsoft.Extensions.Logging;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Models.SupportAgentInvite.Events;

namespace SupportDesk.Application.Models.SupportAgentInvites.Event.SupportAgentInviteRevoked;

internal sealed class LogSupportAgentInviteRevokedDomainEventHandler
    : IDomainEventHandler<SupportAgentInviteRevokedDomainEvent>
{
    private readonly ILogger<LogSupportAgentInviteRevokedDomainEventHandler> _logger;

    public LogSupportAgentInviteRevokedDomainEventHandler(ILogger<LogSupportAgentInviteRevokedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(SupportAgentInviteRevokedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Support agent invite with id '{SupportAgentId}' revoked.", domainEvent.SupportAgentInviteId.IdValue);       
        
        return Task.CompletedTask;
    }
}