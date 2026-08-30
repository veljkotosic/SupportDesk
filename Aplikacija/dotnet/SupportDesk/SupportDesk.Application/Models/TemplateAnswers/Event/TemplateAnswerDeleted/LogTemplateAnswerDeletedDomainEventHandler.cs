using Microsoft.Extensions.Logging;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Models.TemplateAnswer.Events;

namespace SupportDesk.Application.Models.TemplateAnswers.Event.TemplateAnswerDeleted;

internal sealed class LogTemplateAnswerDeletedDomainEventHandler
    : IDomainEventHandler<TemplateAnswerDeletedDomainEvent>
{
    private readonly ILogger<LogTemplateAnswerDeletedDomainEventHandler> _logger;

    public LogTemplateAnswerDeletedDomainEventHandler(ILogger<LogTemplateAnswerDeletedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(TemplateAnswerDeletedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Template answer with id '{TemplateAnswerId}' deleted.'", domainEvent.TemplateAnswerId.IdValue);
        
        return Task.CompletedTask;       
    }
}