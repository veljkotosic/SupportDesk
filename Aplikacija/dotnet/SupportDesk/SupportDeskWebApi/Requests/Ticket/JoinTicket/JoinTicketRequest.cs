using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Ticket.JoinTicket;

public record JoinTicketRequest(Guid TicketId) : IRequest;