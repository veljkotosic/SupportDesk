using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Abstract.ValueObject;

namespace SupportDesk.Domain.Models.Ticket.ValueObjects;

public sealed record TicketLastMessageAt : AbstractValueObject
{
    public DateTime LastMessageAtValue { get; init; }
    
    public TicketLastMessageAt(DateTime LastMessageAtValue)
    {
        this.LastMessageAtValue = LastMessageAtValue;
        ValidateValueObject();
    }

    public override ICollection<IRule> GetValidationRules()
    {
        return [];
    }
}