using SupportDesk.Application.Abstract.Auth.Permission;

namespace SupportDesk.Application.Abstract.Query;

/// <summary>
/// Provides a base implementation for handling queries that return a <typeparamref name="TQueryResult"/>.
/// Orchestrates permission checking and query execution.
/// </summary>
/// <typeparam name="TQuery">The type of query to handle. Must implement <see cref="IQuery{TQueryResult}"/>.</typeparam>
/// <typeparam name="TQueryResult">The type of result produced by handling the query. Must implement <see cref="IQueryResult"/>.</typeparam>
public abstract class AbstractQueryHandler<TQuery, TQueryResult> 
    : IQueryHandler<TQuery, TQueryResult>
    where TQuery : IQuery<TQueryResult>
    where TQueryResult : IQueryResult
{
    private readonly PermissionChecker _permissionChecker;

    /// <summary>
    /// Initializes a new instance of the <see cref="AbstractQueryHandler{TQuery, TQueryResult}"/> class.
    /// </summary>
    /// <param name="permissionChecker">The service used to check required permissions before execution.</param>
    protected AbstractQueryHandler(PermissionChecker permissionChecker)
    {
        _permissionChecker = permissionChecker;
    }

    /// <summary>
    /// Executes the query pipeline asynchronously: verifies permissions and retrieves the query result.
    /// </summary>
    /// <param name="query">The query instance containing input criteria and parameters.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during processing.</param>
    /// <returns>A <see cref="Task{TResult}"/> containing the execution result of type <typeparamref name="TQueryResult"/>.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.</exception>
    public async Task<TQueryResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default)
    {
        await _permissionChecker.CheckAsync(query.GetRequiredPermissions(), cancellationToken);
        
        return await ExecuteAsync(query, cancellationToken);
    }
    
    /// <summary>
    /// Executes the core query logic to retrieve data after permission checks have passed.
    /// </summary>
    /// <param name="query">The query being handled.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous handling operation, containing the result.</returns>
    protected abstract Task<TQueryResult> ExecuteAsync(TQuery query, CancellationToken cancellationToken);
}