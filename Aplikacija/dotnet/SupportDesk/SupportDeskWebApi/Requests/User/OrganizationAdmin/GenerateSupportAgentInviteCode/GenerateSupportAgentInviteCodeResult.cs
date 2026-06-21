using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.User.OrganizationAdmin.GenerateSupportAgentInviteCode;

public record GenerateSupportAgentInviteCodeResult(string Code) : IRequestResult;