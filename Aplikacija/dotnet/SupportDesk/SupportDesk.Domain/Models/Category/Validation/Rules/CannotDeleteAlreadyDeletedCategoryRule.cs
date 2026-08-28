using SupportDesk.Domain.Abstract.Validation.Rule;

namespace SupportDesk.Domain.Models.Category.Validation.Rules;

public sealed class CannotDeleteAlreadyDeletedCategoryRule : AbstractRule, IRuleMetadata
{
    private readonly Category _category;

    public CannotDeleteAlreadyDeletedCategoryRule(Category category)
    {
        _category = category;
    }

    public static string ErrorCodeString => "cannot_delete_already_deleted_category";
    protected override string ErrorCode => ErrorCodeString;
    protected override string ErrorMessage => $"Category '{_category.Name.NameValue}' already deleted";
    public override bool Validate()
    {
        return _category.DeletedAt is null;
    }

}