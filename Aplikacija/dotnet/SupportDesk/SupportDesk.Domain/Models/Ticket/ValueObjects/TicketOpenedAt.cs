using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Abstract.ValueObject;

namespace SupportDesk.Domain.Models.Ticket.ValueObjects;

public sealed record TicketOpenedAt : AbstractValueObject
{
    public DateTime OpenedAtValue { get; init; }
    
    public TicketOpenedAt(DateTime OpenedAtValue)
    {
        this.OpenedAtValue = OpenedAtValue;
        ValidateValueObject(); 
    }

    public override ICollection<IRule> GetValidationRules()
    {
        return [];
    }
    
    public static bool operator >=(TicketOpenedAt left, TicketOpenedAt right) => left.OpenedAtValue >= right.OpenedAtValue;
    public static bool operator <=(TicketOpenedAt left, TicketOpenedAt right) => left.OpenedAtValue <= right.OpenedAtValue;
}