namespace SupportDesk.Application.Abstract.Query;

/// <summary>
/// Represents the result or data payload produced by executing a query.
/// </summary>
/// <remarks>
/// Acts as a marker interface for query return types used in conjunction with 
/// <see cref="IQuery{TQueryResult}"/> and <see cref="IQueryHandler{TQuery, TQueryResult}"/>.
/// </remarks>
public interface IQueryResult;