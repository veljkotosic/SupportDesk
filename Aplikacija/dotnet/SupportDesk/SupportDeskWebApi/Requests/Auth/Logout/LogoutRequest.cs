using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Auth.Logout;

public record LogoutRequest(string RefreshToken) : IRequest;