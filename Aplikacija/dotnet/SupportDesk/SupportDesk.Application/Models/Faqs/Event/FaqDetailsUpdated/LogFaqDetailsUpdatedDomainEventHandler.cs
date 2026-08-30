using Microsoft.Extensions.Logging;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Models.Faq.Events;

namespace SupportDesk.Application.Models.Faqs.Event.FaqDetailsUpdated;

internal sealed class LogFaqDetailsUpdatedDomainEventHandler
    : IDomainEventHandler<FaqDetailsUpdatedDomainEvent>
{
    private readonly ILogger<LogFaqDetailsUpdatedDomainEventHandler> _logger;

    public LogFaqDetailsUpdatedDomainEventHandler(ILogger<LogFaqDetailsUpdatedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(FaqDetailsUpdatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Faq with id '{FaqId}' details updated. New question: '{FaqQuestion}', New answer: '{FaqAnswer}'",
            domainEvent.FaqId.IdValue,
            domainEvent.NewQuestion.QuestionValue,
            domainEvent.NewAnswer.AnswerValue);
        
        return Task.CompletedTask;
    }
}