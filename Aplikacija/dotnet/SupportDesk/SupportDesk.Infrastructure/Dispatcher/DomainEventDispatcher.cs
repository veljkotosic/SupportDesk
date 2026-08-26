using SupportDesk.Application.Abstract.Dispatcher;
using SupportDesk.Domain.Abstract;

namespace SupportDesk.Infrastructure.Dispatcher;

public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    public Task DispatchAsync(IReadOnlyList<IDomainEvent> domainEvents, CancellationToken cancellationToken = default) 
    {
        throw new NotImplementedException();
    }
}