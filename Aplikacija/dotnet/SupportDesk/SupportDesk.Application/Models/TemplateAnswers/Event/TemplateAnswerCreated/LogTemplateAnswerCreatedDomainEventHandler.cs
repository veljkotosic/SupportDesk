using Microsoft.Extensions.Logging;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Models.TemplateAnswer.Events;

namespace SupportDesk.Application.Models.TemplateAnswers.Event.TemplateAnswerCreated;

internal sealed class LogTemplateAnswerCreatedDomainEventHandler
    : IDomainEventHandler<TemplateAnswerCreatedDomainEvent>
{
    private readonly ILogger<LogTemplateAnswerCreatedDomainEventHandler> _logger;

    public LogTemplateAnswerCreatedDomainEventHandler(ILogger<LogTemplateAnswerCreatedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(TemplateAnswerCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Template answer with id '{TemplateAnswerId}' created.", domainEvent.TemplateAnswerId.IdValue);
        
        return Task.CompletedTask;
    }
}