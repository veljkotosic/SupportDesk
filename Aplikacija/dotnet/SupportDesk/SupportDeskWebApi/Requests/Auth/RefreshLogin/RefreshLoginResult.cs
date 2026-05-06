using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Auth.RefreshLogin;

public record RefreshLoginResult(string AccessToken, string RefreshToken) : IRequestResult;