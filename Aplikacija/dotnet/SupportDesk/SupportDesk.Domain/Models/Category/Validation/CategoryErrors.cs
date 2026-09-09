using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.Category.ValueObjects;

namespace SupportDesk.Domain.Models.Category.Validation;

public sealed class CategoryErrors : AbstractErrors<Category, CategoryId>
{
    public static ValidationError AlreadyDeleted(CategoryName categoryName)
    {
        string code = "already_deleted";
        string message = $"Category '{categoryName.NameValue}' already deleted.";
        
        return CreateValidationError(code, message);
    }
}