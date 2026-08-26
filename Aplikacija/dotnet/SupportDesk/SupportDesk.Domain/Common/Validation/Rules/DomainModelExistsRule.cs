using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Common.ValueObjects;

namespace SupportDesk.Domain.Common.Validation.Rules;

public class DomainModelExistsRule<TModel, TId> : AbstractRule
    where TModel : AbstractDomainModel<TId>
    where TId : DomainId
{
    private readonly TModel? _model;
    private readonly ValidationError _error;

    public DomainModelExistsRule(TModel? model, TId id)
        : this(model, AbstractErrors<TModel, TId>.NotFound(id))
    {
        
    }

    public DomainModelExistsRule(TModel? model, string propertyName, object propertyValue)
        : this(model, AbstractErrors<TModel, TId>.NotFound(propertyName, propertyValue))
    {
    }

    public DomainModelExistsRule(TModel? model, ValidationError error)
    {
        _model = model;
        _error = error;
    }

    protected override string ErrorCode => _error.Code;
    protected override string ErrorMessage => _error.Message;

    public override bool Validate() => _model is not null;
}