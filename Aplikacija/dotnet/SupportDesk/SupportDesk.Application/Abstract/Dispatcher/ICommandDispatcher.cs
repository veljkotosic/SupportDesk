using SupportDesk.Application.Abstract.Command;

namespace SupportDesk.Application.Abstract.Dispatcher;

/// <summary>
/// Defines a dispatcher responsible for routing commands to their corresponding command handlers.
/// </summary>
public interface ICommandDispatcher
{
    /// <summary>
    /// Asynchronously dispatches a command that does not return a result to its matching handler.
    /// </summary>
    /// <param name="command">The command instance to execute.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during execution.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="System.InvalidOperationException">Thrown when no matching handler is registered for the command.</exception>
    /// <exception cref="System.OperationCanceledException">Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.</exception>
    Task DispatchAsync(ICommand command, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Asynchronously dispatches a command that returns a <typeparamref name="TCommandResult"/> to its matching handler.
    /// </summary>
    /// <typeparam name="TCommandResult">The type of result returned by the command execution. Must implement <see cref="ICommandResult"/>.</typeparam>
    /// <param name="command">The command instance to execute.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during execution.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the resulting <typeparamref name="TCommandResult"/>.
    /// </returns>
    /// <exception cref="System.InvalidOperationException">Thrown when no matching handler is registered for the command.</exception>
    /// <exception cref="System.OperationCanceledException">Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.</exception>
    Task<TCommandResult> DispatchAsync<TCommandResult>(ICommand<TCommandResult> command, CancellationToken cancellationToken = default)
        where TCommandResult : ICommandResult;
}