using SupportDeskWebApi.Requests.Abstract;
using SupportDeskWebApi.Data.Entities.Ticket.Enums;

namespace SupportDeskWebApi.Requests.Ticket.GetCustomerTickets;

public record GetCustomerTicketsRequest(
    int Skip,
    int Take,
    string? Search,
    TicketStatus? Status) : IRequest<GetCustomerTicketsResult>;
