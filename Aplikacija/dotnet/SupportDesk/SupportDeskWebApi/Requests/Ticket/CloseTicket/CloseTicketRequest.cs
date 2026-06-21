using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Ticket.CloseTicket;

public record CloseTicketRequest(Guid TicketId) : IRequest;