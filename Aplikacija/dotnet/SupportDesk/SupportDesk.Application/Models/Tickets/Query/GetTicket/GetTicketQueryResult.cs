using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Common.Dtos;

namespace SupportDesk.Application.Models.Tickets.Query.GetTicket;

public sealed record GetTicketQueryResult(DashboardTicketDetailsDto Ticket) : IQueryResult;