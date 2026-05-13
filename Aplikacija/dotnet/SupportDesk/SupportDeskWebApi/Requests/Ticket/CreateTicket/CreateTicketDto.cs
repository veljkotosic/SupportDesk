using SupportDeskWebApi.Data.Entities.Ticket.Enums;

namespace SupportDeskWebApi.Requests.Ticket.CreateTicket;

public record CreateTicketDto(
    Guid Id,
    Guid OrganizationId,
    Guid CategoryId,
    Guid CustomerId,
    Guid? SupportAgentId,
    TicketStatus Status,
    TicketPriority Priority,
    string Subject,
    DateTime OpenedAt,
    DateTime? AssignedAt,
    DateTime? ClosedAt,
    TicketFeedback Feedback);