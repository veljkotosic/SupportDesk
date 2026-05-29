using SupportDeskWebApi.Data.Entities.TicketNotification.Enums;

namespace SupportDeskWebApi.Requests.TicketNotification.Common;

public record TicketNotificationDetailsDto(
    Guid Id,
    Guid OrganizationId,
    Guid TicketId,
    string Text,
    TicketNotificationStatus Status,
    DateTime CreatedAt);