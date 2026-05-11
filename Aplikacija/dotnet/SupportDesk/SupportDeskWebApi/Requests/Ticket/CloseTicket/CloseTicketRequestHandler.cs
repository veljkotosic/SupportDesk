using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Database.UnitOfWork;
using SupportDeskWebApi.Data.Entities.Ticket.Enums;
using SupportDeskWebApi.Data.Entities.Ticket.Repository;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Ticket.CloseTicket;

public class CloseTicketRequestHandler
    : IRequestHandler<CloseTicketRequest>
{
    private readonly IUserContext _userContext;
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CloseTicketRequestHandler(
        IUserContext userContext,
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(CloseTicketRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _userContext.GetCurrentUserId();
        
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);

        if (ticket is null)
        {
            throw new Exception("Ticket not found.");
        }

        if (ticket.Status != TicketStatus.Assigned)
        {
            throw new Exception("You can only close assigned tickets.");
        }
        
        if (ticket.SupportAgentId != userId)
        {
            throw new Exception("You can only close tickets assigned to you.");
        }
        
        ticket.Status = TicketStatus.Closed;
        ticket.ClosedAt = DateTime.UtcNow;       
        
        await _ticketRepository.SaveAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);       
    }
}