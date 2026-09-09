using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Models.Ticket.ValueObjects;

namespace SupportDesk.Domain.Models.Ticket.Validation.Rules;

public sealed class TicketLastMessageCannotOccurBeforeItsOpenedRule : AbstractRule, IRuleMetadata
{
    private readonly TicketOpenedAt _ticketOpenedAt;
    private readonly TicketLastMessageAt? _ticketLastMessageAt;
    
    public TicketLastMessageCannotOccurBeforeItsOpenedRule(
        TicketOpenedAt ticketOpenedAt, 
        TicketLastMessageAt? ticketLastMessageAt)
    {
        _ticketOpenedAt = ticketOpenedAt;
        _ticketLastMessageAt = ticketLastMessageAt;
    }
    
    public static string ErrorCodeString => "ticket_last_message_cannot_occur_before_its_opened";
    protected override string ErrorCode => ErrorCodeString;
    protected override string ErrorMessage => "Last message cannot occur before ticket is opened";
    public override bool Validate()
    {
        if (_ticketLastMessageAt is null)
        {
            return true;
        }
        
        return _ticketOpenedAt.OpenedAtValue <= _ticketLastMessageAt.LastMessageAtValue;
    }

}