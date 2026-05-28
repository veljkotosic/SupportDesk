using Microsoft.AspNetCore.SignalR;
using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Database.UnitOfWork;
using SupportDeskWebApi.Data.Entities.Message;
using SupportDeskWebApi.Data.Entities.Message.Repository;
using SupportDeskWebApi.Data.Entities.Ticket.Enums;
using SupportDeskWebApi.Data.Entities.Ticket.Repository;
using SupportDeskWebApi.Hubs;
using SupportDeskWebApi.Requests.Abstract;
using SupportDeskWebApi.Requests.Ticket.Common;

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
        

        var initialMessage = new Data.Entities.Message.Message
        {
            Id = Guid.NewGuid(),
            OrganizationId = request.OrganizationId,
            TicketId = ticket.Id,
            SenderId = callerId,
            CreatedAt = DateTime.UtcNow,
            Text = request.InitialMessage
        };
        
        ticket.LastMessageAt = initialMessage.CreatedAt;
        
        await _ticketRepository.SaveAsync(ticket, cancellationToken);
        await _messageRepository.SaveAsync(initialMessage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var ticketDto = await _ticketRepository.GetTicketAsync(ticket.Id, cancellationToken);
        
        await _organizationDashboardHubContext.Clients.Group(request.OrganizationId.ToString())
            .SendAsync("NewTicket", ticketDto, cancellationToken);       
            
        return new CreateTicketResult(ticket.Id);
    }
}