using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Auth.Signup;

public record SignupRequest(
    string Username,
    string Email,
    string Password,
    string Role
    ) : IRequest<SignupResult>;