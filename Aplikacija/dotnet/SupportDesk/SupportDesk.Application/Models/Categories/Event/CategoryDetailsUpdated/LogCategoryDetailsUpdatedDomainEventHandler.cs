using Microsoft.Extensions.Logging;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Models.Category.Events;

namespace SupportDesk.Application.Models.Categories.Event.CategoryDetailsUpdated;

internal sealed class LogCategoryDetailsUpdatedDomainEventHandler
    : IDomainEventHandler<CategoryDetailsUpdatedDomainEvent>
{
    private readonly ILogger<LogCategoryDetailsUpdatedDomainEventHandler> _logger;

    public LogCategoryDetailsUpdatedDomainEventHandler(ILogger<LogCategoryDetailsUpdatedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(CategoryDetailsUpdatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Category with id '{CategoryId}' details updated. New name: '{CategoryName}', New description: '{CategoryDescription}'", 
            domainEvent.CategoryId.IdValue,
            domainEvent.NewCategoryName.NameValue,
            domainEvent.NewCategoryDescription.DescriptionValue);       
        
        return Task.CompletedTask;
    }
}