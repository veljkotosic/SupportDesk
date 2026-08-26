namespace SupportDesk.Application.Abstract.Command;

public interface ICommandHandlerContext;

public interface ICommandHandlerContext<TCommandResult>
    where TCommandResult : ICommandResult
{
    TCommandResult? Result { get; set; }
}