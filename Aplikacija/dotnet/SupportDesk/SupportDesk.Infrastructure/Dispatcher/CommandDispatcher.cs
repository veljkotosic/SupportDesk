using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Abstract.Dispatcher;

namespace SupportDesk.Infrastructure.Dispatcher;

public sealed class CommandDispatcher : ICommandDispatcher
{
    public Task DispatchAsync(ICommand command, CancellationToken cancellationToken = default) 
    {
        throw new NotImplementedException();
    }

    public Task<TCommandResult> DispatchAsync<TCommandResult>(ICommand<TCommandResult> command, CancellationToken cancellationToken = default) where TCommandResult : ICommandResult
    {
        throw new NotImplementedException();
    }
}