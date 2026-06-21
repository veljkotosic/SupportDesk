using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Ticket.GetTicketViewInfo;

public record GetTicketViewInfoRequest(Guid TicketId) : IRequest<GetTicketViewInfoResult>;