using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Abstract.ValueObject;
using SupportDesk.Domain.Common.Validation.Rules;

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
}