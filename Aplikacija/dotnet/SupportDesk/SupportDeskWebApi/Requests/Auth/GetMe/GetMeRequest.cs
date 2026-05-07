using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Auth.GetMe;

public record GetMeRequest : IRequest<GetMeResult>;