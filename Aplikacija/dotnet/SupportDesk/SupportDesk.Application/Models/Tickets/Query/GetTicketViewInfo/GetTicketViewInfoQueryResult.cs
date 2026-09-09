using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Models.Tickets.Query.GetTicketViewInfo.Dtos;

namespace SupportDesk.Application.Models.Tickets.Query.GetTicketViewInfo;

public sealed record GetTicketViewInfoQueryResult(TicketViewInfoDto Ticket) : IQueryResult;