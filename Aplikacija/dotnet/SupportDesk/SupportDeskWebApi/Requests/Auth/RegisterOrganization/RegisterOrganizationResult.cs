using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Auth.RegisterOrganization;

public record RegisterOrganizationResult(
    string AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpirationDate)
    : IRequestResult;