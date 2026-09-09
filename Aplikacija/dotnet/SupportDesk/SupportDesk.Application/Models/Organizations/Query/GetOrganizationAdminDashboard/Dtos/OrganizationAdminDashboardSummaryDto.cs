namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationAdminDashboard.Dtos;

public sealed record OrganizationAdminDashboardSummaryDto(
    int OpenTickets,
    int AssignedTickets,
    int ResolvedTickets,
    int SupportAgents);
