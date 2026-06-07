using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Ticket.GetCustomerTickets;

public record GetCustomerTicketsRequest(
    int Skip,
    int Take) : IRequest<GetCustomerTicketsResult>;
