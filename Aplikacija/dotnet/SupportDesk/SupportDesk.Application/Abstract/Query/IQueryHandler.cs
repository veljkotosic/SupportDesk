namespace SupportDesk.Application.Abstract.Query;

/// <summary>
/// Defines a handler responsible for executing a specific <typeparamref name="TQuery"/> and returning a <typeparamref name="TQueryResult"/>.
/// </summary>
/// <typeparam name="TQuery">
/// The type of query to handle. Must implement <see cref="IQuery{TQueryResult}"/>.
/// </typeparam>
/// <typeparam name="TQueryResult">
/// The type of result returned after query execution. Must implement <see cref="IQueryResult"/>.
/// </typeparam>
public interface IQueryHandler<in TQuery, TQueryResult>
    where TQuery : IQuery<TQueryResult>
    where TQueryResult : IQueryResult
{
    /// <summary>
    /// Asynchronously handles and executes the specified <paramref name="query"/>.
    /// </summary>
    /// <param name="query">The query instance containing input criteria and parameters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during processing.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the resulting <typeparamref name="TQueryResult"/>.
    /// </returns>
    /// <exception cref="System.OperationCanceledException">Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.</exception>
    Task<TQueryResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}