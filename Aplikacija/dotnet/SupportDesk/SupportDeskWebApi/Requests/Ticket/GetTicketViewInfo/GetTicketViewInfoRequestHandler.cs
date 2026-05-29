using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Entities.Ticket.Repository;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Ticket.GetTicketViewInfo;

public class GetTicketViewInfoRequestHandler
    : IRequestHandler<GetTicketViewInfoRequest, GetTicketViewInfoResult>
{
    private readonly IUserContext _userContext;
    private readonly ITicketRepository _ticketRepository;

    public GetTicketViewInfoRequestHandler(
        IUserContext userContext,
        ITicketRepository ticketRepository)
    {
        _userContext = userContext;
        _ticketRepository = ticketRepository;
    }

    public async Task<GetTicketViewInfoResult> HandleAsync(GetTicketViewInfoRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _userContext.GetCurrentUserId();
        
        var ticket = await _ticketRepository.GetTicketViewInfoAsync(request.TicketId, cancellationToken);

        if (ticket is null)
        {
            throw new Exception("Ticket not found.");      
        }

        if (ticket.CustomerId != userId)
        {
            throw new UnauthorizedAccessException("You can only view your own tickets.");     
        }
        
        return new GetTicketViewInfoResult(ticket);      
    }
}