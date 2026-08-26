using SupportDesk.Application.Abstract.Auth;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Common.Command.Context;

namespace SupportDesk.Application.Models.Auth.Command.LogoutAll;

public sealed class LogoutAllCommandHandler
    : AbstractCommandHandler<LogoutAllCommand, EmptyCommandHandlerContext>
{
    private readonly IUserContext _userContext;
    private readonly IRefreshTokenManager _refreshTokenManager;
    
    public LogoutAllCommandHandler(
        PermissionChecker permissionChecker,
        IUserContext userContext,
        IRefreshTokenManager refreshTokenManager) 
        : base(permissionChecker)
    {
        _userContext = userContext;
        _refreshTokenManager = refreshTokenManager;
    }

    protected override Task<EmptyCommandHandlerContext> PrepareAsync(LogoutAllCommand command, CancellationToken cancellationToken)
    {
        return Task.FromResult(new EmptyCommandHandlerContext());
    }

    protected override Task ExecuteAsync(LogoutAllCommand command, EmptyCommandHandlerContext context, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetCurrentUserId();
        
        return _refreshTokenManager.RevokeAllAsync(userId, cancellationToken);
    }
}