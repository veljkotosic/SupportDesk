using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Models.User.ValueObjects;

namespace SupportDesk.Domain.Models.Ticket.Validation.Rules;

public sealed class TicketMustBeClosedByTheSameUserWhoWasAssignedToItRule : AbstractRule, IRuleMetadata
{
    private readonly Ticket _ticket;
    private readonly UserId _userId;
    
    public TicketMustBeClosedByTheSameUserWhoWasAssignedToItRule(Ticket ticket, UserId userId)
    {
        _ticket = ticket;
        _userId = userId;
    }
    
    public static string ErrorCodeString => "ticket_must_be_closed_by_the_same_user_who_was_assigned_to_it";
    protected override string ErrorCode => ErrorCodeString;
    protected override string ErrorMessage => "You cannot close this ticket";
    public override bool Validate()
    {
        if (_ticket.SupportAgentId is null)
        {
            return false;
        }
        
        return _ticket.SupportAgentId == _userId;
    }

}