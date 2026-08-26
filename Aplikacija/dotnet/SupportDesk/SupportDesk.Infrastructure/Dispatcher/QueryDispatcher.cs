using Microsoft.Extensions.DependencyInjection;
using SupportDesk.Application.Abstract.Dispatcher;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Common.Dispatcher;

namespace SupportDesk.Infrastructure.Dispatcher;

public sealed class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public QueryDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task<TQueryResult> DispatchAsync<TQueryResult>(IQuery<TQueryResult> query, CancellationToken cancellationToken = default) where TQueryResult : IQueryResult
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TQueryResult));
        
        dynamic handler = ResolveHandler(handlerType, query);
        
        return await handler.Handle((dynamic)query, cancellationToken);
    }
    
    private dynamic ResolveHandler(Type handlerType, object dispatchingObject)
    {
        dynamic handler;

        try
        {
            handler = _serviceProvider.GetRequiredService(handlerType);
        }
        catch (InvalidOperationException serviceProviderException)
        {
            throw new DispatchException(dispatchingObject, serviceProviderException);
        }

        return handler;
    }
}