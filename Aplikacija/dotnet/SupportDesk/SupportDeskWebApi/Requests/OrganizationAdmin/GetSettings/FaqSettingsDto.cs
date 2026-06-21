namespace SupportDeskWebApi.Requests.OrganizationAdmin.GetSettings;

public record FaqSettingsDto(Guid Id, string Question, string Answer);
