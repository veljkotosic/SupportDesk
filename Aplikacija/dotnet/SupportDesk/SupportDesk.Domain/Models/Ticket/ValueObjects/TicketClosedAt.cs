using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Abstract.ValueObject;

namespace SupportDesk.Domain.Models.Ticket.ValueObjects;

public sealed record TicketClosedAt : AbstractValueObject
{
    public DateTime ClosedAtValue { get; init; }
    
    public TicketClosedAt(DateTime ClosedAtValue)
    {
        this.ClosedAtValue = ClosedAtValue;
        ValidateValueObject();
    }

    public override ICollection<IRule> GetValidationRules()
    {
        return [];
    }
    
    public static bool operator >=(TicketClosedAt left, TicketClosedAt right) => left.ClosedAtValue >= right.ClosedAtValue;
    public static bool operator <=(TicketClosedAt left, TicketClosedAt right) => left.ClosedAtValue <= right.ClosedAtValue;
}