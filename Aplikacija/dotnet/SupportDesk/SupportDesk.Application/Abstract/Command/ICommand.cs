namespace SupportDesk.Application.Abstract.Command;

/// <summary>
/// Represents a command operation that intends to mutate the application state without returning a business result.
/// </summary>
/// <remarks>
/// Commands represent intent in a CQRS architecture and are processed by an 
/// <see cref="ICommandHandler{TCommand}"/>.
/// </remarks>
public interface ICommand : IUseCase;

/// <summary>
/// Represents a command operation that mutates the application state and yields a <typeparamref name="TCommandResult"/>.
/// </summary>
/// <typeparam name="TCommandResult">
/// The type of result produced upon successful execution of the command. Must implement <see cref="ICommandResult"/>.
/// </typeparam>
/// <remarks>
/// While commands typically focus on state changes, this interface supports scenarios where generating 
/// identifiers, tokens, or execution outcomes is required by the caller.
/// </remarks>
public interface ICommand<TCommandResult> : IUseCase
    where TCommandResult : ICommandResult;