using SupportDeskWebApi.Data.Entities.Ticket.Repository;
using SupportDeskWebApi.Requests.Abstract;
using SupportDeskWebApi.Requests.Ticket.Common;

namespace SupportDeskWebApi.Requests.Ticket.GetTicket;

public class GetTicketRequestHandler
    : IRequestHandler<GetTicketRequest, GetTicketResult>
{
    private readonly ITicketRepository _ticketRepository;

    public GetTicketRequestHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<GetTicketResult> HandleAsync(GetTicketRequest request, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetTicketAsync(request.TicketId, cancellationToken);

        if (ticket is null)
        {
            throw new Exception("Ticket not found.");       
        }

        return new GetTicketResult(ticket);
    }
}