using Microsoft.Extensions.Logging;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Models.Faq.Events;

namespace SupportDesk.Application.Models.Faqs.Event.FaqDeleted;

internal sealed class LogFaqDeletedDomainEventHandler
    : IDomainEventHandler<FaqDeletedDomainEvent>
{
    private readonly ILogger<LogFaqDeletedDomainEventHandler> _logger;

    public LogFaqDeletedDomainEventHandler(ILogger<LogFaqDeletedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(FaqDeletedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Faq with id '{FaqId}' deleted.", domainEvent.FaqId.IdValue);
        
        return Task.CompletedTask;
    }
}