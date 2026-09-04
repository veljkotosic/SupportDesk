using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.UserContext;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.Message;
using SupportDesk.Domain.Models.Message.Repository;
using SupportDesk.Domain.Models.Message.Validation.Rules;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.Ticket.Repository;
using SupportDesk.Domain.Models.Ticket.ValueObjects;
using SupportDesk.Domain.Models.User.Repository;
using SupportDesk.Domain.Models.User.ValueObjects;

namespace SupportDesk.Application.Models.Messages.Command.SendMessage;

internal sealed class SendMessageCommandHandler
    : AbstractCommandHandler<SendMessageCommand, SendMessageCommandResult, SendMessageCommandHandlerContext>
{
    private readonly TimeProvider _timeProvider;
    private readonly IUserContext _userContext;
    private readonly IUserRepository _userRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public SendMessageCommandHandler(
        PermissionChecker permissionChecker,
        TimeProvider timeProvider,
        IUserContext userContext,
        IUserRepository userRepository,
        ITicketRepository ticketRepository,
        IMessageRepository messageRepository,
        IUnitOfWork unitOfWork) 
        : base(permissionChecker)
    {
        _timeProvider = timeProvider;
        _userContext = userContext;
        _userRepository = userRepository;
        _ticketRepository = ticketRepository;
        _messageRepository = messageRepository;
        _unitOfWork = unitOfWork;
    }

    protected override async Task<SendMessageCommandHandlerContext> PrepareAsync(SendMessageCommand command, CancellationToken cancellationToken)
    {
        var ticketId = new TicketId(command.TicketId);
        
        var ticket = await _ticketRepository.GetByIdAsync(ticketId, cancellationToken);

        var userId = new UserId(_userContext.GetCurrentUserId());
        
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        
        return new SendMessageCommandHandlerContext(ticket, ticketId, user!);      
    }

    protected override async Task<SendMessageCommandResult> ExecuteAsync(SendMessageCommand command, SendMessageCommandHandlerContext context, CancellationToken cancellationToken)
    {
        var message = Message.Create(
            context.Ticket!.OrganizationId.IdValue,
            context.TicketId.IdValue,
            context.User.Id.IdValue,
            command.Text,
            _timeProvider);
        
        await _messageRepository.SaveAsync(message, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return new SendMessageCommandResult(message.Id.IdValue);      
    }

    public override IReadOnlyList<ICollection<IRule>> GetValidationStages(SendMessageCommandHandlerContext context)
    {
        return [
            [
                new DomainModelExistsRule<Ticket, TicketId>(context.Ticket, context.TicketId)
            ],
            [
                new UserCanSendMessageToTicketRule(context.Ticket!, context.User)
            ]
        ];
    }
}