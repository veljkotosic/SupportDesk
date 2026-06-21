using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Auth.RefreshLogin;

public record RefreshLoginRequest(string RefreshToken) : IRequest<RefreshLoginResult>;