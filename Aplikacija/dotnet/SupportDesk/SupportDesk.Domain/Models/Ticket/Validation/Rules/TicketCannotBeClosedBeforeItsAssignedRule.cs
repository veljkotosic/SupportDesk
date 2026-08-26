using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Models.Ticket.ValueObjects;

namespace SupportDesk.Domain.Models.Ticket.Validation.Rules;

public sealed class TicketCannotBeClosedBeforeItsAssignedRule : AbstractRule, IRuleMetadata
{
    private readonly TicketAssignedAt? _ticketAssignedAt;
    private readonly TicketClosedAt? _ticketClosedAt;
    
    public TicketCannotBeClosedBeforeItsAssignedRule(
        TicketAssignedAt? ticketAssignedAt, 
        TicketClosedAt? ticketClosedAt)
    {
        _ticketAssignedAt = ticketAssignedAt;
        _ticketClosedAt = ticketClosedAt;
    }
    
    public static string ErrorCodeString => "ticket_cannot_be_closed_before_its_assigned";
    protected override string ErrorCode => ErrorCodeString;
    protected override string ErrorMessage => "Ticket cannot be closed before it is assigned";
    public override bool Validate()
    {
        if (_ticketClosedAt is null)
        {
            return true;
        }

        if (_ticketAssignedAt is null)
        {
            return false;
        }

        return _ticketAssignedAt.AssignedAtValue < _ticketClosedAt.ClosedAtValue;
    }

}