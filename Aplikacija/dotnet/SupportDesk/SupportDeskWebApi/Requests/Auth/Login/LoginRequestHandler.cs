using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Database.UnitOfWork;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Auth.Login;

public class LoginRequestHandler : IRequestHandler<LoginRequest, LoginResult>
{
    private readonly ITokenProvider _tokenProvider;
    private readonly IRefreshTokenManager _refreshTokenManager;
    private readonly IAuthService _authService;
    private readonly IUnitOfWork _unitOfWork;

    public LoginRequestHandler(
        ITokenProvider tokenProvider,
        IRefreshTokenManager refreshTokenManager,
        IAuthService authService,
        IUnitOfWork unitOfWork)
    {
        _tokenProvider = tokenProvider;
        _refreshTokenManager = refreshTokenManager;
        _authService = authService;
        _unitOfWork = unitOfWork;
    }

    public async Task<LoginResult> HandleAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var loginResult = await _authService.LoginWithEmailAndPasswordAsync(request.Email, request.Password, cancellationToken);

        var accessToken = _tokenProvider.GenerateAccessToken(loginResult.User, loginResult.Roles);
        var refreshTokenValue = _tokenProvider.GenerateRefreshToken();
        
        var refreshToken = await _refreshTokenManager.AddAsync(refreshTokenValue, loginResult.User.Id, loginResult.Roles[0], cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginResult(accessToken, refreshTokenValue, refreshToken.ExpiresAt);
    }
}