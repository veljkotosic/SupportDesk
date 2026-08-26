namespace SupportDesk.Application.Abstract.Command;

/// <summary>
/// Defines a handler responsible for executing a specific <typeparamref name="TCommand"/>.
/// </summary>
/// <typeparam name="TCommand">
/// The type of command to handle. Must implement <see cref="ICommand"/>.
/// </typeparam>
public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// Asynchronously handles and executes the specified <paramref name="command"/>.
    /// </summary>
    /// <param name="command">The command instance containing input parameters and execution payload.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during processing.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous handling operation.</returns>
    /// <exception cref="System.OperationCanceledException">Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.</exception>
    Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a handler responsible for executing a <typeparamref name="TCommand"/> and returning a <typeparamref name="TCommandResult"/>.
/// </summary>
/// <typeparam name="TCommand">
/// The type of command to handle. Must implement <see cref="ICommand{TCommandResult}"/>.
/// </typeparam>
/// <typeparam name="TCommandResult">
/// The type of result returned after command execution. Must implement <see cref="ICommandResult"/>.
/// </typeparam>
public interface ICommandHandler<in TCommand, TCommandResult>
    where TCommand : ICommand<TCommandResult>
    where TCommandResult : ICommandResult
{
    /// <summary>
    /// Asynchronously handles and executes the specified <paramref name="command"/>.
    /// </summary>
    /// <param name="command">The command instance containing input parameters and execution payload.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during processing.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation, containing the resulting <typeparamref name="TCommandResult"/>.
    /// </returns>
    Task<TCommandResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);   
}