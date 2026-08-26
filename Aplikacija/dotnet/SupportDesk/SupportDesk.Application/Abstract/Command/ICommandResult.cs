namespace SupportDesk.Application.Abstract.Command;

/// <summary>
/// Represents the result or outcome produced by executing a command.
/// </summary>
/// <remarks>
/// Acts as a marker interface for command return types used in conjunction with 
/// <see cref="ICommand{TCommandResult}"/> and <see cref="ICommandHandler{TCommand, TCommandResult}"/>.
/// </remarks>
public interface ICommandResult;