using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Auth.Login;

public record LoginResult(
    string AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpirationDate)
    : IRequestResult;