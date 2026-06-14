namespace SupportDeskWebApi.Requests.OrganizationAdmin.GetSupportAgents;

public record SupportAgentDto(
    Guid Id,
    string UserName,
    string Email,
    int OpenTickets,
    int ResolvedTickets,
    DateTime JoinedAt);
