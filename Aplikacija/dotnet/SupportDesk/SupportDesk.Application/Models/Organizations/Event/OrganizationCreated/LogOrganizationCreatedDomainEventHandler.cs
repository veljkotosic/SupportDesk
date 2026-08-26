using Microsoft.Extensions.Logging;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Models.Organization.Events;

namespace SupportDesk.Application.Models.Organizations.Event.OrganizationCreated;

public sealed class LogOrganizationCreatedDomainEventHandler : IDomainEventHandler<OrganizationCreatedDomainEvent>
{
    private readonly ILogger<LogOrganizationCreatedDomainEventHandler> _logger;

    public LogOrganizationCreatedDomainEventHandler(ILogger<LogOrganizationCreatedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(OrganizationCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Organization with id '{OrganizationId}' created.", domainEvent.OrganizationId.IdValue);
        
        return Task.CompletedTask;
    }
}