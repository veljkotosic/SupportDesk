using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Abstract.ValueObject;

namespace SupportDesk.Domain.Common.ValueObjects;

public record DomainId : AbstractValueObject
{
    public Guid IdValue { get; init; }

    public DomainId(Guid IdValue)
    {
        this.IdValue = IdValue;
        ValidateValueObject();
    }

    public override ICollection<IRule> GetValidationRules()
    {
        return [];
    }
}