namespace SupportDesk.Application.Abstract.Query;

/// <summary>
/// Represents a query operation that retrieves data without mutating the application state.
/// </summary>
/// <typeparam name="TQueryResult">
/// The type of result produced by executing the query. Must implement <see cref="IQueryResult"/>.
/// </typeparam>
/// <remarks>
/// Queries represent read-only operations in a CQRS architecture and are processed by an 
/// <see cref="IQueryHandler{TQuery, TQueryResult}"/>.
/// </remarks>
public interface IQuery<TQueryResult> : IUseCase
    where TQueryResult : IQueryResult;