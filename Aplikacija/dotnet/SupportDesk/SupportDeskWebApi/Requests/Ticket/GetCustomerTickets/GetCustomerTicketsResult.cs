using SupportDeskWebApi.Requests.Abstract;
using SupportDeskWebApi.Requests.Ticket.Common;

namespace SupportDeskWebApi.Requests.Ticket.GetCustomerTickets;

public record GetCustomerTicketsResult(
    List<TicketDetailsDto> Tickets,
    int TotalCount,
    int AllCount,
    int OpenCount,
    int AssignedCount,
    int ClosedCount) : IRequestResult;
