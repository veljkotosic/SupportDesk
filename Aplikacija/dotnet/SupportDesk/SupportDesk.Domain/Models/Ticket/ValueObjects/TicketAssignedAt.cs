using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Abstract.ValueObject;

namespace SupportDesk.Domain.Models.Ticket.ValueObjects;

public sealed record TicketAssignedAt : AbstractValueObject
{
    public DateTime AssignedAtValue { get; init; }
    
    public TicketAssignedAt(DateTime AssignedAtValue)
    {
        this.AssignedAtValue = AssignedAtValue;
        ValidateValueObject();
    }

    public override ICollection<IRule> GetValidationRules()
    {
        return [];
    }
}