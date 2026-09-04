using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Ticket.ValueObjects;

namespace SupportDesk.Domain.Models.Ticket.Validation;

public sealed class TicketErrors : AbstractErrors<Ticket, TicketId>
{
    public static ValidationError CannotAssign()
    {
        var errorCode = "ticket_cannot_be_assigned";
        var message = "This ticket cannot be assigned.";
        
        return CreateValidationError(errorCode, message);
    }
    
    public static ValidationError CannotClose()
    {
        var errorCode = "ticket_cannot_be_closed";
        var message = "This ticket cannot be closed.";
        
        return CreateValidationError(errorCode, message);
    }
}