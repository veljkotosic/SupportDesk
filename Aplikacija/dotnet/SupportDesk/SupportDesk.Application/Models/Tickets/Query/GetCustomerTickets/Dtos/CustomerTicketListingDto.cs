using SupportDesk.Application.Common.Dtos;
using SupportDesk.Domain.Models.Ticket.Enums;

namespace SupportDesk.Application.Models.Tickets.Query.GetCustomerTickets.Dtos;

public sealed record CustomerTicketListingDto(
    Guid Id,
    Guid OrganizationId,
    string OrganizationName,
    Guid CategoryId,
    string CategoryName,
    Guid CustomerId,
    string CustomerUserName,
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
    IReadOnlyList<TicketNotificationDetailsDto> UnreadNotifications);