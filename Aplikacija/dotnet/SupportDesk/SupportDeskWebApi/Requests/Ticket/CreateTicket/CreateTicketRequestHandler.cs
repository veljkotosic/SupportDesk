using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Database.UnitOfWork;
using SupportDeskWebApi.Data.Entities.Message;
using SupportDeskWebApi.Data.Entities.Message.Repository;
using SupportDeskWebApi.Data.Entities.Ticket.Enums;
using SupportDeskWebApi.Data.Entities.Ticket.Repository;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Ticket.CreateTicket;

public class CreateTicketRequestHandler
    : IRequestHandler<CreateTicketRequest, CreateTicketResult>
{
    private readonly IUserContext _userContext;
    private readonly ITicketRepository _ticketRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTicketRequestHandler(
        IUserContext userContext,
        ITicketRepository ticketRepository,
        IMessageRepository messageRepository,
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _ticketRepository = ticketRepository;
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
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

        var initialMessage = new Message
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
            
        return new CreateTicketResult(ticket.Id);
    }
}