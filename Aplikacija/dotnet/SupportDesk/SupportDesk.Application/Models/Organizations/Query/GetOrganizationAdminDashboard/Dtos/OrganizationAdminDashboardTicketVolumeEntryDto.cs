namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationAdminDashboard.Dtos;

public record OrganizationAdminDashboardTicketVolumeEntryDto(DateOnly Date, int Opened, int Resolved);