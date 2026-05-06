using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Auth.GetMe;

public record GetMeResult(string UserId, string UserName) : IRequestResult;