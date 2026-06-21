using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Ticket.GetTicket;

public record GetTicketRequest(Guid TicketId) : IRequest<GetTicketResult>;