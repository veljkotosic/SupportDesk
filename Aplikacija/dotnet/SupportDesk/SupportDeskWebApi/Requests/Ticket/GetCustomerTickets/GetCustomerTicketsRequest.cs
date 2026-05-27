using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Ticket.GetCustomerTickets;

public record GetCustomerTicketsRequest() : IRequest<GetCustomerTicketsResult>;