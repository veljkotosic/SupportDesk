namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationSupportAgentsSummary.Dtos;

public sealed record SupportAgentSummary(
    Guid Id,
    string UserName,
    string Email,
    int OpenTickets,
    int ResolvedTickets,
    DateTime JoinedAt);