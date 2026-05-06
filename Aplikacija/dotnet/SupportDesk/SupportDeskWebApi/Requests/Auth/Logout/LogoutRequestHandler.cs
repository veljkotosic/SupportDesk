using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Auth.Logout;

public class LogoutRequestHandler : IRequestHandler<LogoutRequest>
{
    private readonly IRefreshTokenManager _refreshTokenManager;
    private readonly IUserContext _userContext;

    public LogoutRequestHandler(IRefreshTokenManager refreshTokenManager, IUserContext userContext)
    {
        _refreshTokenManager = refreshTokenManager;
        _userContext = userContext;
    }

    public async Task HandleAsync(LogoutRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _userContext.GetCurrentUserId();
        
        var refreshToken = await _refreshTokenManager.GetByValueAsync(request.RefreshToken, cancellationToken);

        if (refreshToken.UserId != userId)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        await _refreshTokenManager.RevokeAsync(request.RefreshToken, cancellationToken);
    }
}