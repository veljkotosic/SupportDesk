using SupportDeskWebApi.Data.Entities.User;

namespace SupportDeskWebApi.Auth.Abstract;

public interface ITokenProvider
{
    string GenerateAccessToken(User user, IList<UserRole> roles);
    string GenerateRefreshToken();
}