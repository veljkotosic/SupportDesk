using SupportDesk.Application.Abstract.Auth;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Models.User.Validation.Rules;

namespace SupportDesk.Application.Models.Auth.Command.Logout;

public sealed class LogoutCommandHandler
    : AbstractCommandHandler<LogoutCommand, LogoutCommandHandlerContext>
{
    private readonly IUserContext _userContext;
    private readonly IRefreshTokenManager _refreshTokenManager;
    
    public LogoutCommandHandler(
        PermissionChecker permissionChecker,
        IUserContext userContext,
        IRefreshTokenManager refreshTokenManager) 
        : base(permissionChecker)
    {
        _userContext = userContext;
        _refreshTokenManager = refreshTokenManager;
    }

    protected override async Task<LogoutCommandHandlerContext> PrepareAsync(LogoutCommand command, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetCurrentUserId();
        
        var refreshToken = await _refreshTokenManager.GetByValueAsync(command.RefreshToken, cancellationToken);
        
        return new LogoutCommandHandlerContext(userId, refreshToken);
    }

    protected override async Task ExecuteAsync(LogoutCommand command, LogoutCommandHandlerContext context, CancellationToken cancellationToken)
    {
        await _refreshTokenManager.RevokeAsync(command.RefreshToken, cancellationToken);
    }

    public override IReadOnlyList<ICollection<IRule>> GetValidationStages(LogoutCommandHandlerContext context)
    {
        return [
            [
                new UserCanOnlyLogoutHimselfRule(context.UserId, context.RefreshToken.UserId)
            ]
        ];
    }
}