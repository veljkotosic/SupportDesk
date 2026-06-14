namespace SupportDeskWebApi.Requests.OrganizationAdmin.GetDashboard;

public record TicketVolumeEntryDto(DateOnly Date, int Opened, int Resolved);
