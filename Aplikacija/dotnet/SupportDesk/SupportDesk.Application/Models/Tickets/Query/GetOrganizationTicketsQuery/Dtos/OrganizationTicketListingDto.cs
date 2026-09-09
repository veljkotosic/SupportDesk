using SupportDesk.Domain.Models.Ticket.Enums;

namespace SupportDesk.Application.Models.Tickets.Query.GetOrganizationTicketsQuery.Dtos;

public sealed record OrganizationTicketListingDto(
    Guid Id,
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
    DateTime? LastMessageAt);