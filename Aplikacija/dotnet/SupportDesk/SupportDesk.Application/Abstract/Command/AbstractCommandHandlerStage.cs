using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Abstract.Validation.Rule;

namespace SupportDesk.Application.Abstract.Command;

public abstract class AbstractCommandHandlerStage<TCommand, TCommandHandlerContext>
    : ICommandHandlerStage<TCommand, TCommandHandlerContext>
    where TCommand : ICommand
    where TCommandHandlerContext : ICommandHandlerContext
{
    public async Task<TCommandHandlerContext> HandleAsync(TCommand command, TCommandHandlerContext context, CancellationToken cancellationToken = default)
    {
        var updatedContext = await ExecuteAsync(command, context, cancellationToken);
        
        (this as IValidatable<TCommandHandlerContext>).Validate(updatedContext);
        
        return updatedContext;
    }

    public virtual ICollection<IRule> GetValidationRules(TCommandHandlerContext context)
    {
        return [];
    }
    
    protected abstract Task<TCommandHandlerContext> ExecuteAsync(TCommand command, TCommandHandlerContext context, CancellationToken cancellationToken);
}

public abstract class AbstractCommandHandlerStage<TCommand, TCommandResult, TCommandHandlerContext>
    : ICommandHandlerStage<TCommand, TCommandResult, TCommandHandlerContext>
    where TCommand : ICommand<TCommandResult>
    where TCommandResult : ICommandResult
    where TCommandHandlerContext : ICommandHandlerContext<TCommandResult>
{
    public async Task<TCommandHandlerContext> HandleAsync(TCommand command, TCommandHandlerContext context, CancellationToken cancellationToken = default)
    {
        var updatedContext = await PrepareAsync(command, context, cancellationToken);
        
        (this as IValidatable<TCommandHandlerContext>).Validate(updatedContext);
        
        updatedContext = await ExecuteAsync(command, updatedContext, cancellationToken);
        
        return updatedContext;
    }

    public virtual ICollection<IRule> GetValidationRules(TCommandHandlerContext context)
    {
        return [];
    }
    
    protected abstract Task<TCommandHandlerContext> PrepareAsync(TCommand command, TCommandHandlerContext context, CancellationToken cancellationToken);
    protected abstract Task<TCommandHandlerContext> ExecuteAsync(TCommand command, TCommandHandlerContext context, CancellationToken cancellationToken);
}