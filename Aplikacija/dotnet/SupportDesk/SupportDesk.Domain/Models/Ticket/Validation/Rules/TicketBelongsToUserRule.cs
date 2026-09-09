using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Models.User.ValueObjects;

namespace SupportDesk.Domain.Models.Ticket.Validation.Rules;

public sealed class TicketBelongsToUserRule : AbstractRule, IRuleMetadata
{
    private readonly Ticket _ticket;
    private readonly UserId _userId;

    public TicketBelongsToUserRule(Ticket ticket, UserId userId)
    {
        _ticket = ticket;
        _userId = userId;
    }
    
    public static string ErrorCodeString => "ticket_doesnt_belong_to_user";
    protected override string ErrorCode => ErrorCodeString;
    protected override string ErrorMessage => "This ticket is not opened by you.";
    public override bool Validate()
    {
        return _ticket.CustomerId == _userId;
    }
}