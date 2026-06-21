using SupportDeskWebApi.Data.Entities.User;

namespace SupportDeskWebApi.Auth;

public record LoginResult(User User, IList<UserRole> Roles);