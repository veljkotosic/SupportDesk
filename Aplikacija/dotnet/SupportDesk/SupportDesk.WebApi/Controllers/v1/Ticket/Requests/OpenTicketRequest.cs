using SupportDesk.Domain.Models.Ticket.Enums;

namespace SupportDesk.WebApi.Controllers.v1.Ticket.Requests;

public sealed record OpenTicketRequest(
    Guid OrganizationId,
    Guid CategoryId,
    TicketPriority Priority,
    string Subject,
    string InitialMessage
    );