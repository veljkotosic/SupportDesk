using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Models.Organization.ValueObjects;
using SupportDesk.Domain.Models.User.Enums;

namespace SupportDesk.Domain.Models.User.Validation.Rules;

public sealed class CustomerCannotBePartOfOrganizationRule : AbstractRule, IRuleMetadata
{
    private readonly OrganizationId? _organizationId;
    private readonly UserRole _userRole;

    public CustomerCannotBePartOfOrganizationRule(OrganizationId? organizationId, UserRole userRole)
    {
        _organizationId = organizationId;
        _userRole = userRole;
    }
    
    public static string ErrorCodeString => "customer_cannot_be_part_of_organization";
    protected override string ErrorCode => ErrorCodeString;
    protected override string ErrorMessage => "Customer cannot be part of organization";
    public override bool Validate()
    {
        if (_userRole == UserRole.Customer && _organizationId is not null)
        {
            return false;
        }
        
        return true;
    }

}