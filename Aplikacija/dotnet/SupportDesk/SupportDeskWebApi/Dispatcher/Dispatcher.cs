using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Dispatcher;

public class Dispatcher : IDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public Dispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task ExecuteAsync(IRequest request, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IRequestHandler<>).MakeGenericType(request.GetType());
        
        await this.Handle(handlerType, request, cancellationToken);
    }

    public async Task<TRequestResult> ExecuteAsync<TRequestResult>(IRequest<TRequestResult> request, CancellationToken cancellationToken = default)
        where TRequestResult : IRequestResult
    {
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TRequestResult));
        
        return await this.HandleWithResult<TRequestResult>(handlerType, request, cancellationToken);
    }
    
    private async Task Handle(Type handlerType, object dispatchingObject, CancellationToken cancellationToken = default)
    {
        dynamic handler = this.ResolveHandler(handlerType, dispatchingObject);
        
        await handler.HandleAsync((dynamic)dispatchingObject, cancellationToken);
    }
    
    private async Task<TResult> HandleWithResult<TResult>(Type handlerType, object dispatchingObject, CancellationToken cancellationToken = default)
    {
        dynamic handler = this.ResolveHandler(handlerType, dispatchingObject);
        
        return await handler.HandleAsync((dynamic)dispatchingObject, cancellationToken);
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