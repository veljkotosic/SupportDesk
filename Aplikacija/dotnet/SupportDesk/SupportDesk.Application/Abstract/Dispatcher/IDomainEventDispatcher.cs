using SupportDesk.Domain.Abstract;

namespace SupportDesk.Application.Abstract.Dispatcher;

/// <summary>
/// Defines a dispatcher responsible for publishing domain events to their respective domain event handlers.
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Asynchronously dispatches a collection of domain events to their corresponding registered handlers.
    /// </summary>
    /// <param name="domainEvents">The read-only list of domain events to publish.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during execution.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous dispatch operation.</returns>
    /// <exception cref="System.OperationCanceledException">Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.</exception>
    Task DispatchAsync(IReadOnlyList<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}