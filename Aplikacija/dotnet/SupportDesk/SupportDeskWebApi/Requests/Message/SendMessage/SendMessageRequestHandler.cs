using Microsoft.AspNetCore.SignalR;
using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Database.UnitOfWork;
using SupportDeskWebApi.Data.Entities.Message.Repository;
using SupportDeskWebApi.Data.Entities.Ticket.Enums;
using SupportDeskWebApi.Data.Entities.Ticket.Repository;
using SupportDeskWebApi.Data.Entities.User;
using SupportDeskWebApi.Data.Entities.User.Repository;
using SupportDeskWebApi.Hubs;
using SupportDeskWebApi.Requests.Abstract;
using SupportDeskWebApi.Requests.Message.Common;

namespace SupportDeskWebApi.Requests.Message.SendMessage;

public class SendMessageRequestHandler
    : IRequestHandler<SendMessageRequest, SendMessageResult>
{
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    private readonly IHubContext<CustomerDashboardHub> _customerDashboardHubContext;
    private readonly IHubContext<OrganizationDashboardHub> _organizationDashboardHubContext;
    private readonly IHubContext<TicketHub> _ticketHubContext;

    public SendMessageRequestHandler(
        IUserContext userContext,
        IUserRepository userRepository,
        ITicketRepository ticketRepository,
        IMessageRepository messageRepository,
        IUnitOfWork unitOfWork,
        IHubContext<CustomerDashboardHub> customerDashboardHubContext,
        IHubContext<OrganizationDashboardHub> organizationDashboardHubContext,
        IHubContext<TicketHub> ticketHubContext)
    {
        _userContext = userContext;
        _userRepository = userRepository;
        _ticketRepository = ticketRepository;
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
        _customerDashboardHubContext = customerDashboardHubContext;
        _organizationDashboardHubContext = organizationDashboardHubContext;
        _ticketHubContext = ticketHubContext;
    }

    public async Task<SendMessageResult> HandleAsync(SendMessageRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _userContext.GetCurrentUserId();
        
        var isSupportAgent = _userContext.GetCurrentUsersOrganizationId() != null;
        
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);
        
        if (ticket is null) 
        {
            throw new Exception("Ticket not found");
        }
        
        if (ticket.Status == TicketStatus.Closed)
        {
            throw new Exception("Cannot send message to closed ticket");
        }
        
        if (ticket.Status == TicketStatus.Open && isSupportAgent)
        {
            throw new Exception("Cannot send message to unassigned ticket");
        }
        
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        var message = new Data.Entities.Message.Message
        {
            Id = Guid.NewGuid(),
            OrganizationId = ticket.OrganizationId,
            TicketId = ticket.Id,
            SenderId = userId,
            Text = request.Text,
            CreatedAt = DateTime.UtcNow
        };
        
        await _messageRepository.SaveAsync(message, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        var messageDto = new MessageDetailsDto(
            message.Id,
            message.SenderId,
            user!.UserName!,
            message.Text,
            message.CreatedAt
        );
        
        await _ticketHubContext.Clients.Group(request.TicketId.ToString())
            .SendAsync("NewMessage", messageDto, cancellationToken);

        if (user!.Type == UserType.Customer)
        {
            await _organizationDashboardHubContext.Clients.Group(ticket.OrganizationId.ToString())
                .SendAsync("NewTicketMessage", messageDto, cancellationToken);
        }
        else if (user.Type is UserType.SupportAgent or UserType.OrganizationAdmin)
        {
            await _customerDashboardHubContext.Clients.Group(ticket.CustomerId.ToString())
                .SendAsync("NewTicketMessage", messageDto, cancellationToken);
        }
        
        return new SendMessageResult(message.Id);       
    }
}