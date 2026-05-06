using SupportDeskWebApi.Data.Database.RefreshToken;
using SupportDeskWebApi.Data.Entities.Organization;
using SupportDeskWebApi.Data.Entities.User;

namespace SupportDeskWebApi.Auth.Abstract;

public interface IAuthService
{
    Task SignUpWithEmailAndPasswordAsync(User user, string password, UserRole role, CancellationToken cancellationToken = default);
    Task<LoginResult> LoginWithEmailAndPasswordAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<LoginResult> LoginWithRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
}