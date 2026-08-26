namespace SupportDesk.Domain.Abstract;

/// <summary>
/// Defines the unit of work contract for coordinating transactions and persisting domain state changes.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Asynchronously commits all pending tracked entity changes and dispatches accumulated domain events.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during execution.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous save operation, containing the number of state entries written to the database.
    /// </returns>
    /// <exception cref="System.OperationCanceledException">Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.</exception>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}