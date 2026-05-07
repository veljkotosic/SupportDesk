using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Auth.RegisterSupportAgent;

public record RegisterSupportAgentResult(
    string AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpirationDate)
    : IRequestResult;