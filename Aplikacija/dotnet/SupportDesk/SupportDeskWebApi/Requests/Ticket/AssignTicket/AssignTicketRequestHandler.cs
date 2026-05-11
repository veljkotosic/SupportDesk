using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Database.UnitOfWork;
using SupportDeskWebApi.Data.Entities.Ticket.Enums;
using SupportDeskWebApi.Data.Entities.Ticket.Repository;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Ticket.AssignTicket;

public class AssignTicketRequestHandler 
    : IRequestHandler<AssignTicketRequest>
{
    private readonly IUserContext _userContext;
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignTicketRequestHandler(
        IUserContext userContext,
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(AssignTicketRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _userContext.GetCurrentUserId();
        
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
        
        if (ticket is null)
        {
            throw new Exception("Ticket not found.");
        }

        if (ticket.Status != TicketStatus.Open)
        {
            throw new Exception("You can be assigned tickets that are open.");
        }
        
        ticket.Status = TicketStatus.Assigned;
        ticket.AssignedAt = DateTime.UtcNow;       
        ticket.SupportAgentId = userId;
        
        await _ticketRepository.SaveAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);      
    }
}