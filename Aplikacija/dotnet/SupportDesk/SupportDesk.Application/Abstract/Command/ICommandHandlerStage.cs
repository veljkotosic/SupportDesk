using SupportDesk.Domain.Abstract.Validation;

namespace SupportDesk.Application.Abstract.Command;

public interface ICommandHandlerStage<in TCommand, TCommandHandlerContext>
    : IValidatable<TCommandHandlerContext>
    where TCommand : ICommand
    where TCommandHandlerContext : ICommandHandlerContext
{
    Task<TCommandHandlerContext> HandleAsync(TCommand command, TCommandHandlerContext context, CancellationToken cancellationToken = default);  
}

public interface ICommandHandlerStage<in TCommand, TCommandResult, TCommandHandlerContext>
    : IValidatable<TCommandHandlerContext>
    where TCommand : ICommand<TCommandResult>
    where TCommandResult : ICommandResult
    where TCommandHandlerContext : ICommandHandlerContext<TCommandResult>
{
    Task<TCommandHandlerContext> HandleAsync(TCommand command, TCommandHandlerContext context, CancellationToken cancellationToken = default);  
}