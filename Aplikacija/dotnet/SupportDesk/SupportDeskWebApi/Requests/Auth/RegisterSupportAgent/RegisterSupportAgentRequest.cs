using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Auth.RegisterSupportAgent;

public record RegisterSupportAgentRequest(
    string Username,
    string Email,
    string Password,
    string Code)
    : IRequest<RegisterSupportAgentResult>;