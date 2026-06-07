using SupportDeskWebApi.Data.Entities.Ticket.Enums;
using SupportDeskWebApi.Data.Entities.TicketNotification;
using SupportDeskWebApi.Requests.TicketNotification.Common;

namespace SupportDeskWebApi.Requests.Ticket.Common;

public record TicketDetailsDto(
    Guid Id,
    Guid OrganizationId,
    string OrganizationName,
    Guid CategoryId,
    string CategoryName,
    Guid CustomerId,
    string CustomerUsername,
    Guid? SupportAgentId,
    string? SupportAgentUsername,
    TicketStatus Status,
    TicketPriority Priority,
    string Subject,
    DateTime OpenedAt,
    DateTime? AssignedAt,
    DateTime? ClosedAt,
    TicketFeedback Feedback,
    DateTime LastMessageAt,
    List<TicketNotificationDetailsDto> UnreadNotifications);
