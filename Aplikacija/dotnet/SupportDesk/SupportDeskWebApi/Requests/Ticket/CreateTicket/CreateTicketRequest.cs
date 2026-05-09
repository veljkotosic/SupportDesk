using SupportDeskWebApi.Data.Entities.Ticket.Enums;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Ticket.CreateTicket;

public record CreateTicketRequest(
    Guid OrganizationId,
    Guid CategoryId,
    TicketPriority Priority,
    string Subject,
    string InitialMessage)
    : IRequest<CreateTicketResult>;