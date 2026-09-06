using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Models.User.Enums;

namespace SupportDesk.Domain.Models.Note.Validation.Rules;

public sealed class UserCanAddNoteToTicketRule : AbstractRule, IRuleMetadata
{
    private readonly Ticket.Ticket _ticket;
    private readonly User.User _author;

    public UserCanAddNoteToTicketRule(Ticket.Ticket ticket, User.User author)
    {
        _ticket = ticket;
        _author = author;
    }
    
    public static string ErrorCodeString => "user_cannot_add_note_to_ticket";
    protected override string ErrorCode => ErrorCodeString;
    protected override string ErrorMessage => "You cannot add note to this ticket.";
    public override bool Validate()
    {
        if (_author.Role == UserRole.OrganizationAdmin)
        {
            return true;
        }
        
        return _ticket.SupportAgentId == _author.Id;
    }

}