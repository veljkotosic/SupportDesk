using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Auth.LogoutAll;

public class LogoutAllRequestHandler : IRequestHandler<LogoutAllRequest>
{
    private readonly IRefreshTokenManager _refreshTokenManager;
    private readonly IUserContext _userContext;

    public LogoutAllRequestHandler(IRefreshTokenManager refreshTokenManager, IUserContext userContext)
    {
        _refreshTokenManager = refreshTokenManager;
        _userContext = userContext;
    }

    public async Task HandleAsync(LogoutAllRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _userContext.GetCurrentUserId();
        
        await _refreshTokenManager.RevokeAllAsync(userId, cancellationToken);
    }
}