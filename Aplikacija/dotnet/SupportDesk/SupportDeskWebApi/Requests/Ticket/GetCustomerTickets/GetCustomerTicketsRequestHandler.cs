using SupportDeskWebApi.Data.Entities.Ticket.Repository;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Ticket.GetCustomerTickets;

public class GetCustomerTicketsRequestHandler
    : IRequestHandler<GetCustomerTicketsRequest, GetCustomerTicketsResult>
{
    private readonly ITicketRepository _ticketRepository;

    public GetCustomerTicketsRequestHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<GetCustomerTicketsResult> HandleAsync(GetCustomerTicketsRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _ticketRepository.GetCustomerTicketsAsync(cancellationToken);

        return new GetCustomerTicketsResult(result);
    }
}