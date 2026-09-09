using Microsoft.Extensions.Logging;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Models.Category.Events;

namespace SupportDesk.Application.Models.Categories.Event.CategoryDeleted;

internal sealed class LogCategoryDeletedDomainEventHandler : IDomainEventHandler<CategoryDeletedDomainEvent>
{
    private readonly ILogger<LogCategoryDeletedDomainEventHandler> _logger;

    public LogCategoryDeletedDomainEventHandler(ILogger<LogCategoryDeletedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(CategoryDeletedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Category with id '{CategoryId}' deleted.", domainEvent.CategoryId.IdValue);
        
        return Task.CompletedTask;
    }
}