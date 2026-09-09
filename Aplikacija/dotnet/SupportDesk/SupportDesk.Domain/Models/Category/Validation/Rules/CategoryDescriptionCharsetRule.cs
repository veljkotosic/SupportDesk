using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Utility.Text;

namespace SupportDesk.Domain.Models.Category.Validation.Rules;

public sealed class CategoryDescriptionCharsetRule : CharsetRule, IRuleMetadata
{
    public CategoryDescriptionCharsetRule(string stringValue) 
        : base(stringValue, GetCategoryDescriptionCharsetRule())
    {
        
    }
    
    private static char[] GetCategoryDescriptionCharsetRule()
    {
        var alphanumericCharset = Charset.GetAlphanumericCharset();
        var specialSymbolsCharset = Charset.GetSpecialSymbolsCharset();

        char[] categoryDescriptionCharsetRule = [..alphanumericCharset, ..specialSymbolsCharset, ' '];

        return categoryDescriptionCharsetRule;
    }

    public static string ErrorCodeString => "category_description_charset";
    
    protected override string ErrorCode => ErrorCodeString;

    protected override string ErrorMessage =>
        "Category description contains invalid characters, only alphanumeric and special characters are allowed";
}