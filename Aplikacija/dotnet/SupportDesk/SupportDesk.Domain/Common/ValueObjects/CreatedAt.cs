using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Abstract.ValueObject;
using SupportDesk.Domain.Common.Validation.Rules;

namespace SupportDesk.Domain.Common.ValueObjects;

public sealed record CreatedAt : AbstractValueObject
{
    public DateTime CreatedAtValue { get; private set; }

    public CreatedAt(DateTime CreatedAtValue)
    {
        this.CreatedAtValue = CreatedAtValue;
        ValidateValueObject();
    }
    
    public override ICollection<IRule> GetValidationRules()
    {
        return 
        [
            new DateInPastRule(CreatedAtValue)
        ];
    }
}