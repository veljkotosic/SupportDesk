using SupportDeskWebApi.Requests.Abstract;
using SupportDeskWebApi.Requests.Ticket.Common;

namespace SupportDeskWebApi.Requests.Ticket.GetTicket;

public record GetTicketResult(TicketDetailsDto TicketDetails): IRequestResult;