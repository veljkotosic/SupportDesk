using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Common.Auth.Permissions;

namespace SupportDesk.Application.Models.Organizations.Query.GetAllOrganizations;

public sealed record GetAllOrganizationsQuery : IQuery<GetAllOrganizationsQueryResult>
{
    public ICollection<Permission> GetRequiredPermissions()
    {
        return [
            Permissions.Organizations.ViewAll
        ];
    }
}