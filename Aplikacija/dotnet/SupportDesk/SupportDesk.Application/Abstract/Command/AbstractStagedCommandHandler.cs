using SupportDesk.Application.Abstract.Auth.Permission;

namespace SupportDesk.Application.Abstract.Command;

public abstract class AbstractStagedCommandHandler<TCommand, TCommandHandlerContext>
    : IStagedCommandHandler<TCommand, TCommandHandlerContext>
    where TCommand : ICommand
    where TCommandHandlerContext : ICommandHandlerContext
{
    private readonly PermissionChecker _permissionChecker;

    protected AbstractStagedCommandHandler(PermissionChecker permissionChecker)
    {
        _permissionChecker = permissionChecker;
    }

    public async Task HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        await _permissionChecker.CheckAsync(command.GetRequiredPermissions(), cancellationToken);
        
        var context = await InitializeContextAsync(command, cancellationToken);
        
        var stages = GetCommandHandlerStages();
        
        foreach (var stage in stages)
        {
            context = await stage.HandleAsync(command, context, cancellationToken);
        }
    }
    
    protected abstract Task<TCommandHandlerContext> InitializeContextAsync(TCommand command, CancellationToken cancellationToken = default);
    public abstract ICollection<ICommandHandlerStage<TCommand, TCommandHandlerContext>> GetCommandHandlerStages();
}

public abstract class AbstractStagedCommandHandler<TCommand, TCommandResult, TCommandHandlerContext>
    : IStagedCommandHandler<TCommand, TCommandResult, TCommandHandlerContext>
    where TCommand : ICommand<TCommandResult>
    where TCommandResult : ICommandResult
    where TCommandHandlerContext : ICommandHandlerContext<TCommandResult>
{
    private readonly PermissionChecker _permissionChecker;

    protected AbstractStagedCommandHandler(PermissionChecker permissionChecker)
    {
        _permissionChecker = permissionChecker;
    }

    public async Task<TCommandResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        await _permissionChecker.CheckAsync(command.GetRequiredPermissions(), cancellationToken);
        
        var context = await InitializeContextAsync(command, cancellationToken);
        
        var stages = GetCommandHandlerStages();
        
        foreach (var stage in stages)
        {
            context = await stage.HandleAsync(command, context, cancellationToken);
            
            if (context.Result is not null)
            {
                return context.Result;
            }
        }
        
        throw new InvalidOperationException("Command handler did not produce a result.");
    }
    
    protected abstract Task<TCommandHandlerContext> InitializeContextAsync(TCommand command, CancellationToken cancellationToken = default);
    public abstract ICollection<ICommandHandlerStage<TCommand, TCommandResult, TCommandHandlerContext>> GetCommandHandlerStages();
}

