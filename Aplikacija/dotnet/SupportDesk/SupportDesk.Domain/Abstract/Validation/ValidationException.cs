namespace SupportDesk.Domain.Abstract.Validation;

public class ValidationException 
    : Exception
{
    public ICollection<ValidationError> ValidationErrors { get; private init; }
    
    public ValidationException(ICollection<ValidationError> validationErrors)
    {
        ValidationErrors = validationErrors;
    }

    public ValidationException(ValidationError validationError)
        : this([validationError])
    {
    }
}