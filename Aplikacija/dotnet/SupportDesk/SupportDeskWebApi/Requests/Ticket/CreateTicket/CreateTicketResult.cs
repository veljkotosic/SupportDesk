using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Ticket.CreateTicket;

public record CreateTicketResult(Guid TicketId) : IRequestResult;