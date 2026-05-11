using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Ticket.AssignTicket;

public record AssignTicketRequest(Guid TicketId) : IRequest;