using Microsoft.Extensions.DependencyInjection;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Abstract.Dispatcher;
using SupportDesk.Application.Common.Dispatcher;

namespace SupportDesk.Infrastructure.Dispatcher;

public sealed class CommandDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public CommandDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task DispatchAsync(ICommand command, CancellationToken cancellationToken = default) 
    {
        var handlerType = typeof(ICommandHandler<>).MakeGenericType(command.GetType());
        
        dynamic handler = ResolveHandler(handlerType, command);
        
        await handler.HandleAsync((dynamic)command, cancellationToken);
    }

    public async Task<TCommandResult> DispatchAsync<TCommandResult>(ICommand<TCommandResult> command, CancellationToken cancellationToken = default) where TCommandResult : ICommandResult
    {
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TCommandResult));
        
        dynamic handler = ResolveHandler(handlerType, command);
        
        return await handler.Handle((dynamic)command, cancellationToken);
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