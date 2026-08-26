using Microsoft.Extensions.Logging;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Models.Category.Events;

namespace SupportDesk.Application.Models.Categories.Event.CategoryCreated;

public sealed class LogCategoryCreatedDomainEventHandler : IDomainEventHandler<CategoryCreatedDomainEvent>
{
    private readonly ILogger<LogCategoryCreatedDomainEventHandler> _logger;
    
    public LogCategoryCreatedDomainEventHandler(ILogger<LogCategoryCreatedDomainEventHandler> logger)
    {
        _logger = logger;
    }
    
    public Task HandleAsync(CategoryCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Category with id '{CategoryId}' created.", domainEvent.CategoryId.IdValue);
        
        return Task.CompletedTask;
    }
}