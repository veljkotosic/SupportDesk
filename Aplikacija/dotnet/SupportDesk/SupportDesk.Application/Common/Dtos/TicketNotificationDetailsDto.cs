using SupportDesk.Domain.Models.TicketNotification.Enums;

namespace SupportDesk.Application.Common.Dtos;

public sealed record TicketNotificationDetailsDto(
    Guid Id,
    Guid OrganizationId,
    Guid TicketId,
    string Text,
    TicketNotificationStatus Status,
    DateTime CreatedAt);