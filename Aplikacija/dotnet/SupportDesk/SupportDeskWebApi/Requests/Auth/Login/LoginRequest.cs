using SupportDeskWebApi.Auth;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Auth.Login;

public record LoginRequest(string Email, string Password) : IRequest<LoginResult>;