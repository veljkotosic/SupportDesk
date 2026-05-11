using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Database.UnitOfWork;
using SupportDeskWebApi.Data.Entities.Ticket.Enums;
using SupportDeskWebApi.Data.Entities.Ticket.Repository;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Ticket.GiveFeedback;

public class GiveFeedbackRequestHandler
    : IRequestHandler<GiveFeedbackRequest>
{
    private readonly IUserContext _userContext;
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GiveFeedbackRequestHandler(
        IUserContext userContext,
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(GiveFeedbackRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _userContext.GetCurrentUserId();
        
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
        
        if (ticket is null)
        {
            throw new Exception("Ticket not found.");
        }
        
        if (ticket.CustomerId != userId)
        {
            throw new Exception("You can only give feedback on your own tickets.");
        }

        if (ticket.Status != TicketStatus.Closed)
        {
            throw new Exception("You can only give feedback on closed tickets.");       
        }
        
        ticket.Feedback = request.Feedback;
        
        await _ticketRepository.SaveAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}