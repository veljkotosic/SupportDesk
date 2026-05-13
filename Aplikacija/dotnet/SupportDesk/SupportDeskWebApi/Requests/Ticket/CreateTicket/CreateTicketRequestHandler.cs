using Microsoft.AspNetCore.SignalR;
using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Database.UnitOfWork;
using SupportDeskWebApi.Data.Entities.Message;
using SupportDeskWebApi.Data.Entities.Message.Repository;
using SupportDeskWebApi.Data.Entities.Ticket.Enums;
using SupportDeskWebApi.Data.Entities.Ticket.Repository;
using SupportDeskWebApi.Hubs;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Ticket.CreateTicket;

public class CreateTicketRequestHandler
    : IRequestHandler<CreateTicketRequest, CreateTicketResult>
{
    private readonly IUserContext _userContext;
    private readonly ITicketRepository _ticketRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    private readonly IHubContext<OrganizationDashboardHub> _organizationDashboardHubContext;

    public CreateTicketRequestHandler(
        IUserContext userContext,
        ITicketRepository ticketRepository,
        IMessageRepository messageRepository,
        IUnitOfWork unitOfWork, 
        IHubContext<OrganizationDashboardHub> organizationDashboardHubContext)
    {
        _userContext = userContext;
        _ticketRepository = ticketRepository;
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
        _organizationDashboardHubContext = organizationDashboardHubContext;
    }

    public async Task<CreateTicketResult> HandleAsync(CreateTicketRequest request, CancellationToken cancellationToken = default)
    {
        var callerId = _userContext.GetCurrentUserId();

        var ticket = new Data.Entities.Ticket.Ticket
        {
            Id = Guid.NewGuid(),
            OrganizationId = request.OrganizationId,
            CategoryId = request.CategoryId,
            CustomerId = callerId,
            Subject = request.Subject,
            Status = TicketStatus.Open,
            OpenedAt = DateTime.UtcNow,
            Priority = request.Priority,
            Feedback = TicketFeedback.None
        };
        
        await _ticketRepository.SaveAsync(ticket, cancellationToken);

        var initialMessage = new Data.Entities.Message.Message
        {
            Id = Guid.NewGuid(),
            OrganizationId = request.OrganizationId,
            TicketId = ticket.Id,
            SenderId = callerId,
            CreatedAt = DateTime.UtcNow,
            Text = request.InitialMessage
        };
        
        await _messageRepository.SaveAsync(initialMessage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var ticketDto = new CreateTicketDto
        (
            ticket.Id,
            ticket.OrganizationId,
            ticket.CategoryId,
            ticket.CustomerId,
            ticket.SupportAgentId,
            ticket.Status,
            ticket.Priority,
            ticket.Subject,
            ticket.OpenedAt,
            ticket.AssignedAt,
            ticket.ClosedAt,
            ticket.Feedback
        );
        
        await _organizationDashboardHubContext.Clients.Group(request.OrganizationId.ToString())
            .SendAsync("NewTicket", ticketDto, cancellationToken);       
            
        return new CreateTicketResult(ticket.Id);
    }
}