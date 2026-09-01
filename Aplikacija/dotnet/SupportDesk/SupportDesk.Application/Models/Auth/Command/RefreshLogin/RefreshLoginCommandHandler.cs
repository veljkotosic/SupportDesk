using SupportDesk.Application.Abstract.Auth;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Common.Command.Context;
using SupportDesk.Domain.Abstract;

namespace SupportDesk.Application.Models.Auth.Command.RefreshLogin;

internal sealed class RefreshLoginCommandHandler
    : AbstractCommandHandler<RefreshLoginCommand, RefreshLoginCommandResult, EmptyCommandHandlerContext>
{
    private readonly TimeProvider _timeProvider;
    private readonly ITokenProvider _tokenProvider;
    private readonly IRefreshTokenManager _refreshTokenManager;
    private readonly IAuthService _authService;
    private readonly IUnitOfWork _unitOfWork;
    
    public RefreshLoginCommandHandler(
        PermissionChecker permissionChecker,
        TimeProvider timeProvider,
        ITokenProvider tokenProvider,
        IRefreshTokenManager refreshTokenManager,
        IAuthService authService,
        IUnitOfWork unitOfWork) 
        : base(permissionChecker)
    {
        _timeProvider = timeProvider;
        _tokenProvider = tokenProvider;
        _refreshTokenManager = refreshTokenManager;
        _authService = authService;
        _unitOfWork = unitOfWork;
    }

    protected override Task<EmptyCommandHandlerContext> PrepareAsync(RefreshLoginCommand command, CancellationToken cancellationToken)
    {
        return Task.FromResult(new EmptyCommandHandlerContext());
    }

    protected override async Task<RefreshLoginCommandResult> ExecuteAsync(RefreshLoginCommand command, EmptyCommandHandlerContext context, CancellationToken cancellationToken)
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
}