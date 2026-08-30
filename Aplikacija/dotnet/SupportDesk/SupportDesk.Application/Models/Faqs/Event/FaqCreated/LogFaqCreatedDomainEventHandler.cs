using Microsoft.Extensions.Logging;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Models.Faq.Events;

namespace SupportDesk.Application.Models.Faqs.Event.FaqCreated;

internal sealed class LogFaqCreatedDomainEventHandler
    : IDomainEventHandler<FaqCreatedDomainEvent>
{
    private readonly ILogger<LogFaqCreatedDomainEventHandler> _logger;

    public LogFaqCreatedDomainEventHandler(ILogger<LogFaqCreatedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(FaqCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Faq with id '{FaqId}' created.", domainEvent.FaqId.IdValue);
        
        return Task.CompletedTask;
    }
}