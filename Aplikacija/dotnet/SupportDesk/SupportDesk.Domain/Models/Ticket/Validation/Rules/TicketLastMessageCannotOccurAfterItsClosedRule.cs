using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Models.Ticket.ValueObjects;

namespace SupportDesk.Domain.Models.Ticket.Validation.Rules;

public sealed class TicketLastMessageCannotOccurAfterItsClosedRule : AbstractRule, IRuleMetadata
{
    private readonly TicketClosedAt? _ticketClosedAt;
    private readonly TicketLastMessageAt? _ticketLastMessageAt;
    
    public TicketLastMessageCannotOccurAfterItsClosedRule(
        TicketClosedAt? ticketClosedAt, 
        TicketLastMessageAt? ticketLastMessageAt)
    {
        _ticketClosedAt = ticketClosedAt;
        _ticketLastMessageAt = ticketLastMessageAt;
    }
    
    public static string ErrorCodeString => "ticket_last_message_cannot_occur_after_its_closed";
    protected override string ErrorCode => ErrorCodeString;
    protected override string ErrorMessage => "Last message cannot occur after ticket is closed";
    public override bool Validate()
    {
        if (_ticketLastMessageAt is null || _ticketClosedAt is null)
        {
            return true;
        }
        
        return _ticketClosedAt.ClosedAtValue >= _ticketLastMessageAt.LastMessageAtValue;
    }

}