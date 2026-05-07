using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.User.OrganizationAdmin.RevokeSupportAgentInviteCode;

public record RevokeSupportAgentInviteCodeRequest(string Code) : IRequest;