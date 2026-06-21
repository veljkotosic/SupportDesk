using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Auth.RegisterCustomer;

public record RegisterCustomerResult(
    string AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpirationDate)
    : IRequestResult;