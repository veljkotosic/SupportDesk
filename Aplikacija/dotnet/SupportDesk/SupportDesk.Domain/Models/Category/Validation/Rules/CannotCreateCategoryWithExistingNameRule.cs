using SupportDesk.Domain.Abstract.Validation.Rule;

namespace SupportDesk.Domain.Models.Category.Validation.Rules;

public sealed class CannotCreateCategoryWithExistingNameRule : AbstractRule, IRuleMetadata
{
    private readonly string _name;
    private readonly Category? _category;
    
    public CannotCreateCategoryWithExistingNameRule(string name, Category? category)
    {
        _name = name;
        _category = category;
    }
    
    public static string ErrorCodeString => "cannot_create_category_with_existing_name";
    protected override string ErrorCode => ErrorCodeString;
    protected override string ErrorMessage => $"Category with name '{_name}' already exists";
    public override bool Validate()
    {
        return _category is null;
    }

}