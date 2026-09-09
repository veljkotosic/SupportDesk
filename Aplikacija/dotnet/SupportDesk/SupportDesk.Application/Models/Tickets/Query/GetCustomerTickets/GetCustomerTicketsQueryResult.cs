using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Models.Tickets.Query.GetCustomerTickets.Dtos;

namespace SupportDesk.Application.Models.Tickets.Query.GetCustomerTickets;

public sealed record GetCustomerTicketsQueryResult(
    IReadOnlyList<CustomerTicketListingDto> Tickets,
    int TotalCount,
    int AllCount,
    int OpenCount,
    int AssignedCount,
    int ClosedCount
    ) : IQueryResult;