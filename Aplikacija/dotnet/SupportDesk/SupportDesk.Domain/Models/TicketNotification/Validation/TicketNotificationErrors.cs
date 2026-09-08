using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.TicketNotification.ValueObjects;

namespace SupportDesk.Domain.Models.TicketNotification.Validation;

public sealed class TicketNotificationErrors : AbstractErrors<TicketNotification, TicketNotificationId>
{
    public static ValidationError AlreadyRead(TicketNotificationId ticketNotificationId)
    {
        var errorCode = "already_read";
        var message = $"Ticket notification with id '{ticketNotificationId.IdValue}' already read.";
        
        return new ValidationError(errorCode, message);
    }
}