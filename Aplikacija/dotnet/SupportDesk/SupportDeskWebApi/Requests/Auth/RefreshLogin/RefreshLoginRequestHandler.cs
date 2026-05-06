using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Database.UnitOfWork;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Auth.RefreshLogin;

public class RefreshLoginRequestHandler : IRequestHandler<RefreshLoginRequest, RefreshLoginResult>
{
    private readonly ITokenProvider _tokenProvider;
    private readonly IRefreshTokenManager _refreshTokenManager;
    private readonly IAuthService _authService;
    private readonly IUnitOfWork _unitOfWork;
    
    public RefreshLoginRequestHandler(ITokenProvider tokenProvider, IRefreshTokenManager refreshTokenManager, IAuthService authService, IUnitOfWork unitOfWork)
    {
        _tokenProvider = tokenProvider;
        _refreshTokenManager = refreshTokenManager;
        _authService = authService;
        _unitOfWork = unitOfWork;
    }

    public async Task<RefreshLoginResult> HandleAsync(RefreshLoginRequest request, CancellationToken cancellationToken = default)
    {
        var refreshToken = await _refreshTokenManager.GetByValueAsync(request.RefreshToken, cancellationToken);
        
        var loginResult = await _authService.LoginWithRefreshTokenAsync(refreshToken, cancellationToken);

        var newAccessToken = _tokenProvider.GenerateAccessToken(loginResult.User, loginResult.Roles);
        var newRefreshTokenValue = _tokenProvider.GenerateRefreshToken();
        
        await _refreshTokenManager.RevokeAsync(refreshToken.Token, cancellationToken);
        var newRefreshToken = await _refreshTokenManager.AddAsync(newRefreshTokenValue, loginResult.User.Id, loginResult.Roles[0], cancellationToken);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return new RefreshLoginResult(newAccessToken, newRefreshTokenValue, newRefreshToken.ExpiresAt);
    }
}