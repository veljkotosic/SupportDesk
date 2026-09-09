using SupportDesk.Domain.Models.Ticket.Enums;

namespace SupportDesk.Application.Models.Tickets.Query.GetTicketViewInfo.Dtos;

public sealed record TicketViewInfoDto(
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
    IReadOnlyList<TicketMessageDetailsDto> Messages,
    IReadOnlyList<TicketNoteDetailsDto> Notes
);