using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Abstract.ValueObject;
using SupportDesk.Domain.Common.Validation.Rules;

namespace SupportDesk.Domain.Common.ValueObjects;

public sealed record DeletedAt : AbstractValueObject
{
    public DateTime DeletedAtValue { get; init; }
    
    public DeletedAt(DateTime DeletedAtValue)
    {
        this.DeletedAtValue = DeletedAtValue;
        ValidateValueObject();
    }
    
    public override ICollection<IRule> GetValidationRules()
    {
        return [];
    }
}