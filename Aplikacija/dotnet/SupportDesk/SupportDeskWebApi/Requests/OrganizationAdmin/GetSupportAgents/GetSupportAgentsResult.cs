using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.OrganizationAdmin.GetSupportAgents;

public record GetSupportAgentsResult(List<SupportAgentDto> Agents) : IRequestResult;
