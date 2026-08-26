using SupportDesk.Application.Abstract.Auth;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Common.Command.Context;
using SupportDesk.Domain.Abstract;

namespace SupportDesk.Application.Models.Auth.Command.Login;

public sealed class LoginCommandHandler
    : AbstractCommandHandler<LoginCommand, LoginCommandResult, EmptyCommandHandlerContext>
{
    private readonly ITokenProvider _tokenProvider;
    private readonly IRefreshTokenManager _refreshTokenManager;
    private readonly IAuthService _authService;
    private readonly IUnitOfWork _unitOfWork;
    
    public LoginCommandHandler(
        PermissionChecker permissionChecker,
        ITokenProvider tokenProvider,
        IRefreshTokenManager refreshTokenManager,
        IAuthService authService,
        IUnitOfWork unitOfWork) 
        : base(permissionChecker)
    {
        _tokenProvider = tokenProvider;
        _refreshTokenManager = refreshTokenManager;
        _authService = authService;
        _unitOfWork = unitOfWork;
    }

    protected override Task<EmptyCommandHandlerContext> PrepareAsync(LoginCommand command, CancellationToken cancellationToken)
    {
        return Task.FromResult(new EmptyCommandHandlerContext());
    }

    protected override async Task<LoginCommandResult> ExecuteAsync(LoginCommand command, EmptyCommandHandlerContext context, CancellationToken cancellationToken)
    {
        var loginResult = await _authService.LoginWithEmailAndPasswordAsync(command.Email, command.Password, cancellationToken);

        var accessToken = _tokenProvider.GenerateAccessToken(loginResult);
        var refreshTokenValue = _tokenProvider.GenerateRefreshToken();
        
        var refreshToken = await _refreshTokenManager.AddAsync(refreshTokenValue, loginResult.Id.IdValue, loginResult.Role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginCommandResult(accessToken, refreshToken);
    }
}