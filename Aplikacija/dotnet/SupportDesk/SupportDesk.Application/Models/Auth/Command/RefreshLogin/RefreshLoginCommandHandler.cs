using SupportDesk.Application.Abstract.Auth;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Common.Command.Context;
using SupportDesk.Application.Models.Auth.Command.Logout;
using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Models.User.Validation.Rules;

namespace SupportDesk.Application.Models.Auth.Command.RefreshLogin;

internal sealed class RefreshLoginCommandHandler
    : AbstractCommandHandler<RefreshLoginCommand, RefreshLoginCommandResult, RefreshLoginCommandHandlerContext>
{
    private readonly TimeProvider _timeProvider;
    private readonly IUserContext _userContext;
    private readonly ITokenProvider _tokenProvider;
    private readonly IRefreshTokenManager _refreshTokenManager;
    private readonly IAuthService _authService;
    private readonly IUnitOfWork _unitOfWork;
    
    public RefreshLoginCommandHandler(
        PermissionChecker permissionChecker,
        TimeProvider timeProvider,
        IUserContext userContext,
        ITokenProvider tokenProvider,
        IRefreshTokenManager refreshTokenManager,
        IAuthService authService,
        IUnitOfWork unitOfWork) 
        : base(permissionChecker)
    {
        _timeProvider = timeProvider;
        _userContext = userContext;
        _tokenProvider = tokenProvider;
        _refreshTokenManager = refreshTokenManager;
        _authService = authService;
        _unitOfWork = unitOfWork;
    }

    protected override async Task<RefreshLoginCommandHandlerContext> PrepareAsync(RefreshLoginCommand command, CancellationToken cancellationToken)
    {
        var userId = _userContext.GetCurrentUserId();
        
        var refreshToken = await _refreshTokenManager.GetByValueAsync(command.RefreshToken, cancellationToken);
        
        return new RefreshLoginCommandHandlerContext(userId, refreshToken);
    }

    protected override async Task<RefreshLoginCommandResult> ExecuteAsync(RefreshLoginCommand command, RefreshLoginCommandHandlerContext context, CancellationToken cancellationToken)
    {
        var refreshToken = await _refreshTokenManager.GetByValueAsync(command.RefreshToken, cancellationToken);
        
        var loginResult = await _authService.LoginWithRefreshTokenAsync(refreshToken, cancellationToken);

        var newAccessToken = _tokenProvider.GenerateAccessToken(loginResult);
        var newRefreshTokenValue = _tokenProvider.GenerateRefreshToken();
        
        await _refreshTokenManager.RevokeAsync(refreshToken.Value, cancellationToken);
        var newRefreshToken = await _refreshTokenManager.AddAsync(newRefreshTokenValue, loginResult.Id.IdValue, loginResult.Role, _timeProvider, cancellationToken);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return new RefreshLoginCommandResult(newAccessToken, newRefreshToken);
    }

    public override IReadOnlyList<ICollection<IRule>> GetValidationStages(RefreshLoginCommandHandlerContext context)
    {
        return
        [
            [
                new UserCanOnlyRefreshHisLoginRule(context.UserId, context.RefreshToken.UserId)
            ]
        ];
    }
}