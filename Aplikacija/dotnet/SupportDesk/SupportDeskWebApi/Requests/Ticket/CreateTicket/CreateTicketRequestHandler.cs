using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Database;
using SupportDeskWebApi.Data.Database.UnitOfWork;
using SupportDeskWebApi.Data.Entities.Message;
using SupportDeskWebApi.Data.Entities.Message.Repository;
using SupportDeskWebApi.Data.Entities.Ticket.Enums;
using SupportDeskWebApi.Data.Entities.Ticket.Repository;
using SupportDeskWebApi.Hubs;
using SupportDeskWebApi.Requests.Abstract;
using SupportDeskWebApi.Requests.Ticket.Common;
using SupportDeskWebApi.Requests.TicketNotification.Common;

namespace SupportDeskWebApi.Requests.Ticket.CreateTicket;

public class CreateTicketRequestHandler
    : IRequestHandler<CreateTicketRequest, CreateTicketResult>
{
    private readonly IUserContext _userContext;
    private readonly SupportDeskDbContext _context;
    private readonly ITicketRepository _ticketRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    private readonly IHubContext<OrganizationDashboardHub> _organizationDashboardHubContext;

    public CreateTicketRequestHandler(
        IUserContext userContext,
        SupportDeskDbContext context,
        ITicketRepository ticketRepository,
        IMessageRepository messageRepository,
        IUnitOfWork unitOfWork, 
        IHubContext<OrganizationDashboardHub> organizationDashboardHubContext)
    {
        _userContext = userContext;
        _context = context;
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

        var ticketDto = await _context.Tickets
            .AsNoTracking()
            .Where(t => t.Id == ticket.Id)
            .Select(t => new TicketDetailsDto(
                t.Id,
                t.OrganizationId,
                t.Organization.Name,
                t.CategoryId,
                t.Category.Name,
                t.CustomerId,
                t.Customer.UserName!,
                t.SupportAgentId,
                t.SupportAgent != null ? t.SupportAgent.UserName : null,
                t.Status,
                t.Priority,
                t.Subject,
                t.OpenedAt,
                t.AssignedAt,
                t.ClosedAt,
                t.Feedback,
                t.LastMessageAt,
                new List<TicketNotificationDetailsDto>()))
            .FirstAsync(cancellationToken);
        
        await _organizationDashboardHubContext.Clients.Group(request.OrganizationId.ToString())
            .SendAsync("NewTicket", ticketDto, cancellationToken);       
            
        return new CreateTicketResult(ticket.Id);
    }
}
