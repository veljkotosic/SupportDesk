using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Auth.Signup;

public record SignupResult(string AccessToken, string RefreshToken) : IRequestResult;