using Microsoft.AspNetCore.SignalR;
using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Database.UnitOfWork;
using SupportDeskWebApi.Data.Entities.Ticket.Enums;
using SupportDeskWebApi.Data.Entities.Ticket.Repository;
using SupportDeskWebApi.Hubs;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Ticket.CloseTicket;

public class CloseTicketRequestHandler
    : IRequestHandler<CloseTicketRequest>
{
    private readonly IUserContext _userContext;
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    private readonly IHubContext<CustomerDashboardHub> _customerDashboardHubContext;
    private readonly IHubContext<TicketHub> _ticketHubContext;

    public CloseTicketRequestHandler(
        IUserContext userContext,
        ITicketRepository ticketRepository,
        IUnitOfWork unitOfWork, 
        IHubContext<CustomerDashboardHub> customerDashboardHubContext,
        IHubContext<TicketHub> ticketHubContext)
    {
        _userContext = userContext;
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
        _customerDashboardHubContext = customerDashboardHubContext;
        _ticketHubContext = ticketHubContext;
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
        
        var ticketClosedInfoDto = new TicketClosedInfoDto(ticket.Id, ticket.ClosedAt);
        
        await _customerDashboardHubContext.Clients.Group(ticket.CustomerId.ToString())
            .SendAsync("TicketClosed", ticketClosedInfoDto, cancellationToken);
        
        await _ticketHubContext.Clients.Group(ticket.Id.ToString())
            .SendAsync("TicketClosed", ticketClosedInfoDto, cancellationToken);
    }
}