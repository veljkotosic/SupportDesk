namespace SupportDesk.Application.Abstract.Command;

public interface IStagedCommandHandler<TCommand, TCommandHandlerContext>
    : ICommandHandler<TCommand>
    where TCommand : ICommand
    where TCommandHandlerContext : ICommandHandlerContext
{
    ICollection<ICommandHandlerStage<TCommand, TCommandHandlerContext>> GetCommandHandlerStages();
}

public interface IStagedCommandHandler<TCommand, TCommandResult, TCommandHandlerContext>
    : ICommandHandler<TCommand, TCommandResult>
    where TCommand : ICommand<TCommandResult>
    where TCommandResult : ICommandResult
    where TCommandHandlerContext : ICommandHandlerContext<TCommandResult>
{
    ICollection<ICommandHandlerStage<TCommand, TCommandResult, TCommandHandlerContext>> GetCommandHandlerStages();
}