using SupportDesk.Domain.Models.Ticket.Enums;

namespace SupportDesk.Application.Common.Dtos;

public sealed record DashboardTicketDetailsDto(
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
    DateTime? LastMessageAt,
    List<TicketNotificationDetailsDto> UnreadNotifications);