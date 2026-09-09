using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Models.Ticket.Enums;
using SupportDesk.Domain.Models.User.Enums;

namespace SupportDesk.Domain.Models.Message.Validation.Rules;

public sealed class UserCanSendMessageToTicketRule : AbstractRule, IRuleMetadata
{
    private readonly Ticket.Ticket _ticket;
    private readonly User.User _sender;

    public UserCanSendMessageToTicketRule(
        Ticket.Ticket ticket,
        User.User sender)
    {
        _ticket = ticket;
        _sender = sender;
    }
    
    public static string ErrorCodeString => "cannot_send_message_to_this_ticket";
    protected override string ErrorCode => ErrorCodeString;
    protected override string ErrorMessage => "Cannot send message to this ticket";
    public override bool Validate()
    {
        return _sender.Role switch
        {
            UserRole.Customer => 
                _ticket.CustomerId == _sender.Id && 
                _ticket.Status is TicketStatus.Open or TicketStatus.Assigned,

            UserRole.SupportAgent => 
                _ticket.SupportAgentId == _sender.Id && 
                _ticket.Status == TicketStatus.Assigned,

            _ => false
        };
    }

}