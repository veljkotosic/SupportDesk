using Microsoft.AspNetCore.SignalR;
using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Database.UnitOfWork;
using SupportDeskWebApi.Data.Entities.Ticket.Enums;
using SupportDeskWebApi.Data.Entities.Ticket.Repository;
using SupportDeskWebApi.Hubs;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Ticket.GiveFeedback;

public class GiveFeedbackRequestHandler
    : IRequestHandler<GiveFeedbackRequest>
{
    private readonly IUserContext _userContext;
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    private readonly IHubContext<OrganizationDashboardHub> _organizationDashboardHubContext;
    private readonly IHubContext<TicketHub> _ticketHubContext;

    public GiveFeedbackRequestHandler(
        IUserContext userContext,
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork,
        IHubContext<OrganizationDashboardHub> organizationDashboardHubContext,
        IHubContext<TicketHub> ticketHubContext)
    {
        _userContext = userContext;
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
        _organizationDashboardHubContext = organizationDashboardHubContext;
        _ticketHubContext = ticketHubContext;
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
        
        var ticketFeedbackInfoDto = new TicketFeedbackInfoDto(ticket.Id, ticket.Feedback);      
        
        await _organizationDashboardHubContext.Clients.Group(ticket.OrganizationId.ToString())
            .SendAsync("TicketFeedback", ticketFeedbackInfoDto, cancellationToken);
        
        await _ticketHubContext.Clients.Group(ticket.Id.ToString())
            .SendAsync("TicketFeedback", ticketFeedbackInfoDto, cancellationToken);       
    }
}
