using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Models.Ticket.ValueObjects;

namespace SupportDesk.Domain.Models.Ticket.Validation.Rules;

public sealed class TicketCannotBeAssignedBeforeItsOpenedRule : AbstractRule, IRuleMetadata
{
    private readonly TicketOpenedAt _ticketOpenedAt;
    private readonly TicketAssignedAt? _ticketAssignedAt;
    
    public TicketCannotBeAssignedBeforeItsOpenedRule(
        TicketOpenedAt ticketOpenedAt, 
        TicketAssignedAt? ticketAssignedAt)
    {
        _ticketOpenedAt = ticketOpenedAt;
        _ticketAssignedAt = ticketAssignedAt;
    }
    
    public static string ErrorCodeString => "ticket_cannot_be_assigned_before_its_opened";
    protected override string ErrorCode => ErrorCodeString;
    protected override string ErrorMessage => "Ticket cannot be assigned before it is opened";
    public override bool Validate()
    {
        if (_ticketAssignedAt is null)
        {
            return true;
        }
        
        return _ticketOpenedAt.OpenedAtValue < _ticketAssignedAt.AssignedAtValue;
    }

}