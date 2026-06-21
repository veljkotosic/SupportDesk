using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Ticket.GetTicketViewInfo;

public record GetTicketViewInfoResult(TicketViewInfoDto Ticket) : IRequestResult;