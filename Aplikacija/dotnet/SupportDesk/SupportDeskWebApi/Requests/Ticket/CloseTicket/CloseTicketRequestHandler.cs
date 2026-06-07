using Microsoft.AspNetCore.SignalR;
using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Database.UnitOfWork;
using SupportDeskWebApi.Data.Entities.Ticket.Enums;
using SupportDeskWebApi.Data.Entities.Ticket.Repository;
using SupportDeskWebApi.Data.Entities.TicketNotification;
using SupportDeskWebApi.Data.Entities.TicketNotification.Enums;
using SupportDeskWebApi.Data.Entities.TicketNotification.Repository;
using SupportDeskWebApi.Hubs;
using SupportDeskWebApi.Requests.Abstract;
using SupportDeskWebApi.Requests.TicketNotification.Common;

namespace SupportDeskWebApi.Requests.Ticket.CloseTicket;

public class CloseTicketRequestHandler
    : IRequestHandler<CloseTicketRequest>
{
    private readonly IUserContext _userContext;
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketNotificationRepository _ticketNotificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    private readonly IHubContext<CustomerDashboardHub> _customerDashboardHubContext;
    private readonly IHubContext<OrganizationDashboardHub> _organizationDashboardHubContext;
    private readonly IHubContext<TicketHub> _ticketHubContext;

    public CloseTicketRequestHandler(
        IUserContext userContext,
        ITicketRepository ticketRepository,
        ITicketNotificationRepository ticketNotificationRepository,
        IUnitOfWork unitOfWork, 
        IHubContext<CustomerDashboardHub> customerDashboardHubContext,
        IHubContext<OrganizationDashboardHub> organizationDashboardHubContext,
        IHubContext<TicketHub> ticketHubContext)
    {
        _userContext = userContext;
        _ticketRepository = ticketRepository;
        _ticketNotificationRepository = ticketNotificationRepository;       
        _unitOfWork = unitOfWork;
        _customerDashboardHubContext = customerDashboardHubContext;
        _organizationDashboardHubContext = organizationDashboardHubContext;
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

        var notification = new Data.Entities.TicketNotification.TicketNotification
        {
            Id = Guid.NewGuid(),
            OrganizationId = ticket.OrganizationId,
            TicketId = ticket.Id,
            Text = "Support agent has closed this ticket.",
            Status = TicketNotificationStatus.Unread,
            CreatedAt = DateTime.UtcNow
        };
        
        await _ticketRepository.SaveAsync(ticket, cancellationToken);
        await _ticketNotificationRepository.SaveAsync(notification, cancellationToken);       
        await _unitOfWork.SaveChangesAsync(cancellationToken);       
        
        var ticketClosedInfoDto = new TicketClosedInfoDto(ticket.Id, userId, ticket.ClosedAt);
        
        var notificationDto = new TicketNotificationDetailsDto(
            notification.Id,
            notification.OrganizationId,
            notification.TicketId,
            notification.Text,
            notification.Status,
            notification.CreatedAt);
        
        await _customerDashboardHubContext.Clients.Group(ticket.CustomerId.ToString())
            .SendAsync("NewTicketNotification", notificationDto, cancellationToken);
        await _customerDashboardHubContext.Clients.Group(ticket.CustomerId.ToString())
            .SendAsync("TicketClosed", ticketClosedInfoDto, cancellationToken);

        await _organizationDashboardHubContext.Clients.Group(ticket.OrganizationId.ToString())
            .SendAsync("TicketClosed", ticketClosedInfoDto, cancellationToken);
        
        await _ticketHubContext.Clients.Group(ticket.Id.ToString())
            .SendAsync("TicketClosed", ticketClosedInfoDto, cancellationToken);
    }
}
