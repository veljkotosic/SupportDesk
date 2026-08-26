using SupportDesk.Application.Abstract.Dispatcher;
using SupportDesk.Application.Abstract.Query;

namespace SupportDesk.Infrastructure.Dispatcher;

public sealed class QueryDispatcher : IQueryDispatcher
{
    public Task<TQueryResult> DispatchAsync<TQueryResult>(IQuery<TQueryResult> query, CancellationToken cancellationToken = default) where TQueryResult : IQueryResult
    {
        throw new NotImplementedException();
    }
}