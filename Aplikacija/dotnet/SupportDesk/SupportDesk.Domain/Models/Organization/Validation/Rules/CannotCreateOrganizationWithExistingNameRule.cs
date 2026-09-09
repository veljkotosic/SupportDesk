using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Models.Organization.ValueObjects;

namespace SupportDesk.Domain.Models.Organization.Validation.Rules;

public sealed class CannotCreateOrganizationWithExistingNameRule : AbstractRule, IRuleMetadata
{
    private readonly OrganizationName _organizationName;
    private readonly Organization? _organization;
    
    public CannotCreateOrganizationWithExistingNameRule(
        OrganizationName organizationName,
        Organization? organization)
    {
        _organizationName = organizationName;
        _organization = organization;
    }
    
    public static string ErrorCodeString => "cannot_create_organization_with_existing_name";
    protected override string ErrorCode => ErrorCodeString;
    protected override string ErrorMessage => "Organization with this name already exists";
    public override bool Validate()
    {
        if (_organization is null)
        {
            return true;
        }
        
        return _organizationName == _organization.Name;
    }

}