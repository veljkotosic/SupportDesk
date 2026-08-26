using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Abstract.Validation.Rule;

namespace SupportDesk.Application.Abstract.Command;

/// <summary>
/// Provides a base implementation for handling commands that do not return a result.
/// Orchestrates permission checking, context preparation, stage-based validation, and execution.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle. Must implement <see cref="ICommand"/>.</typeparam>
/// <typeparam name="TCommandHandlerContext">The type of execution context used by the handler. Must implement <see cref="ICommandHandlerContext"/>.</typeparam>
public abstract class AbstractCommandHandler<TCommand, TCommandHandlerContext>
    : ICommandHandler<TCommand>, IStageValidatable<TCommandHandlerContext>
    where TCommand : ICommand
    where TCommandHandlerContext : ICommandHandlerContext
{
    private readonly PermissionChecker _permissionChecker;

    /// <summary>
    /// Initializes a new instance of the <see cref="AbstractCommandHandler{TCommand, TCommandHandlerContext}"/> class.
    /// </summary>
    /// <param name="permissionChecker">The service used to check required permissions before execution.</param>
    protected AbstractCommandHandler(PermissionChecker permissionChecker)
    {
        _permissionChecker = permissionChecker;
    }

    /// <summary>
    /// Executes the command pipeline asynchronously: checks permissions, prepares the context, runs stage validations, and processes the command.
    /// </summary>
    /// <param name="command">The command instance containing input parameters and payload.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during processing.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.</exception>
    public async Task HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        await _permissionChecker.CheckAsync(command.GetRequiredPermissions(), cancellationToken);
        
        var context = await PrepareAsync(command, cancellationToken);
        
        (this as IStageValidatable<TCommandHandlerContext>).Validate(context);
        
        await HandleInternalAsync(command, context, cancellationToken);
    }

    /// <summary>
    /// Retrieves the collection of validation rule stages to execute against the prepared context.
    /// </summary>
    /// <param name="context">The handler execution context containing data required for validation.</param>
    /// <returns>A read-only list of validation rule stages.</returns>
    public virtual IReadOnlyList<ICollection<IRule>> GetValidationStages(TCommandHandlerContext context) => [];
    
    /// <summary>
    /// Asynchronously prepares the execution context required for command validation and handling.
    /// </summary>
    /// <param name="command">The command being handled.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task{TResult}"/> containing the prepared execution context.</returns>
    protected abstract Task<TCommandHandlerContext> PrepareAsync(TCommand command, CancellationToken cancellationToken);
    
    /// <summary>
    /// Executes the core business logic for handling the command after permission checks and validations have passed.
    /// </summary>
    /// <param name="command">The command being handled.</param>
    /// <param name="context">The validated execution context.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous handling operation.</returns>
    protected abstract Task HandleInternalAsync(TCommand command, TCommandHandlerContext context, CancellationToken cancellationToken);
}

/// <summary>
/// Provides a base implementation for handling commands that return a <typeparamref name="TCommandResult"/>.
/// Orchestrates permission checking, context preparation, stage-based validation, and execution.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle. Must implement <see cref="ICommand{TCommandResult}"/>.</typeparam>
/// <typeparam name="TCommandResult">The type of result produced by handling the command. Must implement <see cref="ICommandResult"/>.</typeparam>
/// <typeparam name="TCommandHandlerContext">The type of execution context used by the handler. Must implement <see cref="ICommandHandlerContext"/>.</typeparam>
public abstract class AbstractCommandHandler<TCommand, TCommandResult, TCommandHandlerContext>
    : ICommandHandler<TCommand, TCommandResult>, IStageValidatable<TCommandHandlerContext>
    where TCommand : ICommand<TCommandResult>
    where TCommandResult : ICommandResult
    where TCommandHandlerContext : ICommandHandlerContext
{
    private readonly PermissionChecker _permissionChecker;

    /// <summary>
    /// Initializes a new instance of the <see cref="AbstractCommandHandler{TCommand, TCommandResult, TCommandHandlerContext}"/> class.
    /// </summary>
    /// <param name="permissionChecker">The service used to check required permissions before execution.</param>
    protected AbstractCommandHandler(PermissionChecker permissionChecker)
    {
        _permissionChecker = permissionChecker;
    }
    
    /// <summary>
    /// Executes the command pipeline asynchronously: checks permissions, prepares the context, runs stage validations, and processes the command.
    /// </summary>
    /// <param name="command">The command instance containing input parameters and payload.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests during processing.</param>
    /// <returns>A <see cref="Task{TResult}"/> containing the execution result of type <typeparamref name="TCommandResult"/>.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled via the <paramref name="cancellationToken"/>.</exception>
    public async Task<TCommandResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default)
    {
        await _permissionChecker.CheckAsync(command.GetRequiredPermissions(), cancellationToken);
        
        var context = await PrepareAsync(command, cancellationToken);
        
        (this as IStageValidatable<TCommandHandlerContext>).Validate(context);
        
        return await HandleInternalAsync(command, context, cancellationToken);
    }

    /// <summary>
    /// Retrieves the collection of validation rule stages to execute against the prepared context.
    /// </summary>
    /// <param name="context">The handler execution context containing data required for validation.</param>
    /// <returns>A read-only list of validation rule stages.</returns>
    public virtual IReadOnlyList<ICollection<IRule>> GetValidationStages(TCommandHandlerContext context) => [];
    
    /// <summary>
    /// Asynchronously prepares the execution context required for command validation and handling.
    /// </summary>
    /// <param name="command">The command being handled.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task{TResult}"/> containing the prepared execution context.</returns>
    protected abstract Task<TCommandHandlerContext> PrepareAsync(TCommand command, CancellationToken cancellationToken);
    
    /// <summary>
    /// Executes the core business logic for handling the command after permission checks and validations have passed.
    /// </summary>
    /// <param name="command">The command being handled.</param>
    /// <param name="context">The validated execution context.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous handling operation, containing the result.</returns>
    protected abstract Task<TCommandResult> HandleInternalAsync(TCommand command, TCommandHandlerContext context, CancellationToken cancellationToken);
}