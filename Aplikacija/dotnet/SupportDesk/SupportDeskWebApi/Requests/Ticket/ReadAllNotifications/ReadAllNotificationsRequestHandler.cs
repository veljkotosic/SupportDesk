using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Database.UnitOfWork;
using SupportDeskWebApi.Data.Entities.Ticket.Repository;
using SupportDeskWebApi.Data.Entities.TicketNotification.Enums;
using SupportDeskWebApi.Data.Entities.TicketNotification.Repository;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Ticket.ReadAllNotifications;

public class ReadAllNotificationsRequestHandler
    : IRequestHandler<ReadAllNotificationsRequest>
{
    private readonly IUserContext _userContext;
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketNotificationRepository _ticketNotificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReadAllNotificationsRequestHandler(
        IUserContext userContext,
        ITicketRepository ticketRepository,
        ITicketNotificationRepository ticketNotificationRepository,
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _ticketRepository = ticketRepository;
        _ticketNotificationRepository = ticketNotificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(ReadAllNotificationsRequest request, CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.TicketId, cancellationToken);

        if (ticket is null)
        {
            throw new Exception("Ticket not found.");     
        }

        if (ticket.CustomerId != _userContext.GetCurrentUserId())
        {
            throw new UnauthorizedAccessException("You can only read notifications for your own tickets.");      
        }
        
        var unreadNotifications = await _ticketNotificationRepository.GetUnreadNotificationsAsync(request.TicketId, cancellationToken);

        foreach (var notification in unreadNotifications)
        {
            notification.Status = TicketNotificationStatus.Read;
            await _ticketNotificationRepository.SaveAsync(notification, cancellationToken);
        }
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);      
    }
}
