namespace SupportDeskWebApi.Requests.OrganizationAdmin.GetDashboard;

public record DashboardAgentDto(Guid Id, string UserName, int OpenTickets);
