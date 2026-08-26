using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Utility.Text;

namespace SupportDesk.Domain.Models.Organization.Validation.Rules;

public sealed class OrganizationNameCharsetRule : CharsetRule, IRuleMetadata
{
    public OrganizationNameCharsetRule(string stringValue) 
        : base(stringValue, GetOrganizationNameCharset())
    {
        
    }
    
    private static char[] GetOrganizationNameCharset()
    {
        var alphanumericCharset = Charset.GetAlphanumericCharset();

        char[] organizationNameCharset = [..alphanumericCharset, '_', ' '];

        return organizationNameCharset;
    }

    public static string ErrorCodeString => "organization_name_charset";
    
    protected override string ErrorCode => ErrorCodeString;

    protected override string ErrorMessage =>
        "Organization name contains invalid characters, only alphanumeric characters and '_' are allowed";
}