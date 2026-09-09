using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.UserContext;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.Ticket;
using SupportDesk.Domain.Models.Ticket.Repository;
using SupportDesk.Domain.Models.Ticket.Validation.Rules;
using SupportDesk.Domain.Models.Ticket.ValueObjects;
using SupportDesk.Domain.Models.TicketNotification.Repository;
using SupportDesk.Domain.Models.User.ValueObjects;

namespace SupportDesk.Application.Models.Tickets.Command.ReadAllNotifications;

internal sealed class ReadAllNotificationsCommandHandler
    : AbstractCommandHandler<ReadAllNotificationsCommand, ReadAllNotificationsCommandHandlerContext>
{
    private readonly IUserContext _userContext;
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketNotificationRepository _ticketNotificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public ReadAllNotificationsCommandHandler(
        PermissionChecker permissionChecker,
        IUserContext userContext,
        ITicketRepository ticketRepository,
        ITicketNotificationRepository ticketNotificationRepository,
        IUnitOfWork unitOfWork) 
        : base(permissionChecker)
    {
        _userContext = userContext;
        _ticketRepository = ticketRepository;
        _ticketNotificationRepository = ticketNotificationRepository;
        _unitOfWork = unitOfWork;
    }

    protected override async Task<ReadAllNotificationsCommandHandlerContext> PrepareAsync(ReadAllNotificationsCommand command, CancellationToken cancellationToken)
    {
        var ticketId = new TicketId(command.TicketId);
        
        var ticket = await _ticketRepository.GetByIdAsync(ticketId, cancellationToken);
        
        var userId = new UserId(_userContext.GetCurrentUserId());
        
        return new ReadAllNotificationsCommandHandlerContext(ticket, ticketId, userId);      
    }

    protected override async Task ExecuteAsync(ReadAllNotificationsCommand command, ReadAllNotificationsCommandHandlerContext context, CancellationToken cancellationToken)
    {
        var notifications = await _ticketNotificationRepository.GetUnreadNotificationsByTicketIdAsync(
                context.TicketId,
                cancellationToken);

        if (notifications.Count > 0)
        {
            foreach (var notification in notifications)
            {
                notification.Read();
                await _ticketNotificationRepository.SaveAsync(notification, cancellationToken);
            }
        
            await _unitOfWork.SaveChangesAsync(cancellationToken);  
        }   
    }

    public override IReadOnlyList<ICollection<IRule>> GetValidationStages(ReadAllNotificationsCommandHandlerContext context)
    {
        return
        [
            [
                new DomainModelExistsRule<Ticket, TicketId>(context.Ticket, context.TicketId)
            ],
            [
                new TicketBelongsToUserRule(context.Ticket!, context.UserId)
            ]
        ];
    }
}