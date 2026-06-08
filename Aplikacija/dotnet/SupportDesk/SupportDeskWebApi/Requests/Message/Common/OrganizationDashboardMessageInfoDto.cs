namespace SupportDeskWebApi.Requests.Message.Common;

public record OrganizationDashboardMessageInfoDto(
    Guid TicketId,
    DateTime CreatedAt);
