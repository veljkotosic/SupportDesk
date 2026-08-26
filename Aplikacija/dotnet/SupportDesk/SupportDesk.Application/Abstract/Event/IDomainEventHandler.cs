using SupportDesk.Domain.Abstract;

namespace SupportDesk.Application.Abstract.Event;

/// <summary>
/// Defines a handler responsible for processing a specific type of <typeparamref name="TDomainEvent"/>.
/// </summary>
/// <typeparam name="TDomainEvent">
/// The type of domain event to handle. Must implement <see cref="IDomainEvent"/>.
/// </typeparam>
public interface IDomainEventHandler<in TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    /// <summary>
    /// Asynchronously processes the specified <paramref name="domainEvent"/>.
    /// </summary>
    /// <param name="domainEvent">The domain event instance to process.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during processing.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous handling operation.</returns>
    /// <exception cref="System.OperationCanceledException">Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.</exception>
    Task HandleAsync(TDomainEvent domainEvent, CancellationToken cancellationToken = default);
}