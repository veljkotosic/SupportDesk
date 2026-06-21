using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Entities.Message.Repository;
using SupportDeskWebApi.Data.Entities.Ticket.Repository;
using SupportDeskWebApi.Data.Entities.User.Repository;
using SupportDeskWebApi.Requests.Abstract;
using SupportDeskWebApi.Requests.Message.Common;

namespace SupportDeskWebApi.Requests.Message.GetMessage;

public class GetMessageRequestHandler
    : IRequestHandler<GetMessageRequest, GetMessageResult>
{
    private readonly IUserContext _userContext;
    private readonly ITicketRepository _ticketRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMessageRepository _messageRepository;

    public GetMessageRequestHandler(
        IUserContext userContext,
        IUserRepository userRepository,
        ITicketRepository ticketRepository,
        IMessageRepository messageRepository)
    {
        _userContext = userContext;
        _userRepository = userRepository;
        _ticketRepository = ticketRepository;
        _messageRepository = messageRepository;
    }

    public async Task<GetMessageResult> HandleAsync(GetMessageRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _userContext.GetCurrentUserId();
        
        var message = await _messageRepository.GetByIdAsync(request.MessageId, cancellationToken);

        if (message is null)
        {
            throw new Exception("Message not found.");
        }
        
        var ticket = await _ticketRepository.GetByIdAsync(message.TicketId, cancellationToken);

        if (ticket is null)
        {
            throw new Exception("Ticket not found.");
        }
        
        if (ticket.CustomerId != userId)
        {
            throw new UnauthorizedAccessException("You can only view messages from your own tickets.");
        }
        
        var user = await _userRepository.GetByIdAsync(message.SenderId, cancellationToken);

        var messageDto = new MessageDetailsDto(
            message.Id,
            message.SenderId,
            user!.UserName!,
            message.Text,
            message.CreatedAt);
        
        return new GetMessageResult(messageDto);
    }
}
