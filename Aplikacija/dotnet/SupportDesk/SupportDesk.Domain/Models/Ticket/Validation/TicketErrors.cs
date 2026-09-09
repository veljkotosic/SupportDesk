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

    public static ValidationError CannotGiveFeedbackIfTicketIsNotClosed()
    {
        var errorCode = "cannot_give_feedback";
        var message = "Feedback cannot be given if ticket is not closed.";
        
        return CreateValidationError(errorCode, message);
    }

    public static ValidationError FeedbackAlreadyGiven()
    {
        var errorCode = "feedback_already_given";
        var message = "Feedback has already been given to this ticket.";
        
        return CreateValidationError(errorCode, message);   
    }
    
    public static ValidationError InvalidFeedback()
    {
        var errorCode = "invalid_ticket_feedback";
        var message = "Ticket feedback must be either 'Helpful' or 'Unhelpful'.";
        return CreateValidationError(errorCode, message);
    }
}