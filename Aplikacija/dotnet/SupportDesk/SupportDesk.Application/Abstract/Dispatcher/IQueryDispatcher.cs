using SupportDesk.Application.Abstract.Query;

namespace SupportDesk.Application.Abstract.Dispatcher;

/// <summary>
/// Defines a dispatcher responsible for routing queries to their corresponding query handlers and returning results.
/// </summary>
public interface IQueryDispatcher
{
    /// <summary>
    /// Asynchronously dispatches a query to its matching handler and retrieves the resulting <typeparamref name="TQueryResult"/>.
    /// </summary>
    /// <typeparam name="TQueryResult">The type of data result returned by the query. Must implement <see cref="IQueryResult"/>.</typeparam>
    /// <param name="query">The query instance containing input parameters and criteria.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during execution.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the resulting <typeparamref name="TQueryResult"/>.
    /// </returns>
    /// <exception cref="System.InvalidOperationException">Thrown when no matching handler is registered for the query.</exception>
    /// <exception cref="System.OperationCanceledException">Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.</exception>
    Task<TQueryResult> DispatchAsync<TQueryResult>(IQuery<TQueryResult> query, CancellationToken cancellationToken = default)
        where TQueryResult : IQueryResult;
}