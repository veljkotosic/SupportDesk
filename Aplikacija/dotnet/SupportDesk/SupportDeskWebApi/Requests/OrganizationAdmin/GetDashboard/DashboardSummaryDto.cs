namespace SupportDeskWebApi.Requests.OrganizationAdmin.GetDashboard;

public record DashboardSummaryDto(int OpenTickets, int AssignedTickets, int ResolvedTickets, int SupportAgents);
