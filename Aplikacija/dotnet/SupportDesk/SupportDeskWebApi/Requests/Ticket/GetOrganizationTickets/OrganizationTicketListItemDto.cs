using SupportDeskWebApi.Data.Entities.Ticket.Enums;

namespace SupportDeskWebApi.Requests.Ticket.GetOrganizationTickets;

public record OrganizationTicketListItemDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    Guid CustomerId,
    string CustomerUsername,
    string CustomerEmail,
    Guid? SupportAgentId,
    string? SupportAgentUsername,
    TicketStatus Status,
    TicketPriority Priority,
    string Subject,
    DateTime LastMessageAt);
