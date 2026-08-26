using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Utility.Text;

namespace SupportDesk.Domain.Models.Category.Validation.Rules;

public sealed class CategoryNameCharsetRule : CharsetRule, IRuleMetadata
{
    public CategoryNameCharsetRule(string stringValue) 
        : base(stringValue, GetCategoryNameCharsetRule())
    {
        
    }
    
    private static char[] GetCategoryNameCharsetRule()
    {
        var alphanumericCharset = Charset.GetAlphanumericCharset();

        char[] categoryNameCharsetRule = [..alphanumericCharset, '_'];

        return categoryNameCharsetRule;
    }

    public static string ErrorCodeString => "category_name_charset";
    
    protected override string ErrorCode => ErrorCodeString;

    protected override string ErrorMessage =>
        "Category name contains invalid characters, only alphanumeric characters and '_' are allowed";
}