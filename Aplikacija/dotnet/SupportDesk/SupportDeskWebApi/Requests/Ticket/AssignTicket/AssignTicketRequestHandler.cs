using Microsoft.AspNetCore.SignalR;
using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Database.UnitOfWork;
using SupportDeskWebApi.Data.Entities.Ticket.Enums;
using SupportDeskWebApi.Data.Entities.Ticket.Repository;
using SupportDeskWebApi.Data.Entities.TicketNotification;
using SupportDeskWebApi.Data.Entities.TicketNotification.Enums;
using SupportDeskWebApi.Data.Entities.TicketNotification.Repository;
using SupportDeskWebApi.Data.Entities.User.Repository;
using SupportDeskWebApi.Hubs;
using SupportDeskWebApi.Requests.Abstract;
using SupportDeskWebApi.Requests.TicketNotification.Common;

namespace SupportDeskWebApi.Requests.Ticket.AssignTicket;

public class AssignTicketRequestHandler 
    : IRequestHandler<AssignTicketRequest>
{
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketNotificationRepository _ticketNotificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    private readonly IHubContext<CustomerDashboardHub> _customerDashboardHubContext;
    private readonly IHubContext<TicketHub> _ticketHubContext;

    public AssignTicketRequestHandler(
        IUserContext userContext,
        IUserRepository userRepository,
        ITicketRepository ticketRepository,
        ITicketNotificationRepository ticketNotificationRepository,
        IUnitOfWork unitOfWork,
        IHubContext<CustomerDashboardHub> customerDashboardHubContext,
        IHubContext<TicketHub> ticketHubContext)
    {
        _userContext = userContext;
        _userRepository = userRepository;       
        _ticketRepository = ticketRepository;
        _ticketNotificationRepository = ticketNotificationRepository;
        _unitOfWork = unitOfWork;
        _customerDashboardHubContext = customerDashboardHubContext;
        _ticketHubContext = ticketHubContext;
    }

    public async Task HandleAsync(AssignTicketRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _userContext.GetCurrentUserId();
        
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        
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

        var notification = new Data.Entities.TicketNotification.TicketNotification
        {
            Id = Guid.NewGuid(),
            OrganizationId = ticket.OrganizationId,
            TicketId = ticket.Id,
            Text = "Support agent is reviewing your ticket.",
            Status = TicketNotificationStatus.Unread,
            CreatedAt = DateTime.UtcNow
        };
        
        await _ticketRepository.SaveAsync(ticket, cancellationToken);
        await _ticketNotificationRepository.SaveAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);   
        
        var ticketAssignedInfoDto = new TicketAssignedInfoDto(
            ticket.Id,
            user!.Id,
            user.UserName!,
            ticket.AssignedAt);
        
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
            .SendAsync("TicketAssigned", ticketAssignedInfoDto, cancellationToken);
        
        await _ticketHubContext.Clients.Group(ticket.Id.ToString())
            .SendAsync("TicketAssigned", ticketAssignedInfoDto, cancellationToken);
    }
}