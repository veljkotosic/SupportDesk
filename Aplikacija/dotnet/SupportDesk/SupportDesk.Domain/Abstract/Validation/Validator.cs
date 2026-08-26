using SupportDesk.Domain.Abstract.Validation.Rule;

namespace SupportDesk.Domain.Abstract.Validation;

public class Validator
{
    private readonly ICollection<IRule> _validationRules;
    private readonly string _objectName;

    public Validator(ICollection<IRule> validationRules, string objectName)
    {
        _validationRules = validationRules;
        _objectName = objectName;
    }
    
    public bool Success { get; private set; } = false;

    private ValidationException? _validationException = null;
    
    public ValidationException GetValidationException()
    {
        if (_validationException is null)
        {
            return new ValidationException([]);
        }
        return _validationException;
    }

    public void Validate()
    {
        List<ValidationError> errors = [];
        
        foreach (var validationRule in _validationRules)
        {
            validationRule.ObjectName = _objectName;
            
            if (!validationRule.Validate())
            {
                errors.Add(validationRule.GetError());
            }
        }

        if (errors.Count != 0)
        {
            Success = false;
            _validationException = new ValidationException(errors.ToArray());
        }
        else
        {
            Success = true;
            _validationException = null;
        }
    }
}