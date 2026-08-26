using Microsoft.Extensions.DependencyInjection;
using SupportDesk.Application.Abstract.Dispatcher;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Abstract;

namespace SupportDesk.Infrastructure.Dispatcher;

public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync(IReadOnlyList<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handlers = _serviceProvider.GetServices(handlerType);

            foreach (dynamic? handler in handlers)
            {
                if (handler is not null)
                {
                    await handler.HandleAsync((dynamic)domainEvent, cancellationToken);
                }
            }
        }
    }
}