using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Models.Category.ValueObjects;

namespace SupportDesk.Domain.Models.Category.Validation.Rules;

public sealed class CannotUpdateCategoryNameWithExistingOneRule : AbstractRule, IRuleMetadata
{
    private readonly Category _category;
    private readonly CategoryName? _newCategoryName;
    private readonly Category? _categoryWithSameName;

    public CannotUpdateCategoryNameWithExistingOneRule(
        Category category, 
        CategoryName? newCategoryName,
        Category? categoryWithSameName)
    {
        _category = category;
        _newCategoryName = newCategoryName;
        _categoryWithSameName = categoryWithSameName;
    }
    
    public static string ErrorCodeString => "cannot_update_category_name_with_existing_one";
    protected override string ErrorCode => ErrorCodeString;
    protected override string ErrorMessage => $"Category with this name already exists";
    public override bool Validate()
    {
        if (_newCategoryName is null)
        {
            return true;
        }
        
        if (_categoryWithSameName is null)
        {
            return true;
        }

        return _category.Id == _categoryWithSameName.Id;
    }

}