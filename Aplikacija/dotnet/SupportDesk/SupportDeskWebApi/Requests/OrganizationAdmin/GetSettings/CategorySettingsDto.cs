namespace SupportDeskWebApi.Requests.OrganizationAdmin.GetSettings;

public record CategorySettingsDto(Guid Id, string Name, string Description, int TicketCount);
