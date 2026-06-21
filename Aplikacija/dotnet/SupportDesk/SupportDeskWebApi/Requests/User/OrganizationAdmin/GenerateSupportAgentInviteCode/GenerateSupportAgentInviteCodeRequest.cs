using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.User.OrganizationAdmin.GenerateSupportAgentInviteCode;

public record GenerateSupportAgentInviteCodeRequest(
    string Email) 
    : IRequest<GenerateSupportAgentInviteCodeResult>;