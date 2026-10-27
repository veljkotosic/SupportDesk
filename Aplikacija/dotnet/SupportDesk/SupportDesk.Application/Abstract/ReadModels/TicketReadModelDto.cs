using SupportDesk.Application.Common.Dtos;
using SupportDesk.Domain.Models.Ticket.Enums;

namespace SupportDesk.Application.Abstract.ReadModels;

public sealed record TicketReadModelDto(
    Guid Id,
    Guid OrganizationId,
    string OrganizationName,
    Guid CategoryId,
    string CategoryName,
    Guid CustomerId,
    string CustomerUserName,
    string CustomerEmail,
    Guid? SupportAgentId,
    string? SupportAgentUserName,
    TicketStatus Status,
    TicketPriority Priority,
    TicketFeedback Feedback,
    string Subject,
    DateTime OpenedAt,
    DateTime? AssignedAt,
    DateTime? ClosedAt,
    DateTime? LastMessageAt,
    IReadOnlyList<TicketNotificationDetailsDto> UnreadNotifications
);
