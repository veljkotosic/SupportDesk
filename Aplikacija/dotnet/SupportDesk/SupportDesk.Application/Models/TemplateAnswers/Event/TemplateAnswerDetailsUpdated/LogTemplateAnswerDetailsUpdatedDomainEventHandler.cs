using Microsoft.Extensions.Logging;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Models.TemplateAnswer.Events;

namespace SupportDesk.Application.Models.TemplateAnswers.Event.TemplateAnswerDetailsUpdated;

internal sealed class LogTemplateAnswerDetailsUpdatedDomainEventHandler
    : IDomainEventHandler<TemplateAnswerDetailsUpdatedDomainEvent>
{
    private readonly ILogger<LogTemplateAnswerDetailsUpdatedDomainEventHandler> _logger;

    public LogTemplateAnswerDetailsUpdatedDomainEventHandler(ILogger<LogTemplateAnswerDetailsUpdatedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(TemplateAnswerDetailsUpdatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Template answer with id '{TemplateAnswerId}' details updated. New title: '{TemplateAnswerTitle}', New text: '{TemplateAnswerText}'",
            domainEvent.TemplateAnswerId.IdValue,
            domainEvent.NewTemplateAnswerTitle.TitleValue,
            domainEvent.NewTemplateAnswerText.TextValue);
        
        return Task.CompletedTask;
    }
}