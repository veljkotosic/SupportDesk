using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Common.Auth.Permissions;

namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationAdminDashboard;

public sealed record GetOrganizationAdminDashboardQuery : IQuery<GetOrganizationAdminDashboardQueryResult>
{
    public ICollection<Permission> GetRequiredPermissions()
    {
        return [
            Permissions.Organizations.ViewAdminDashboard
        ];
    }
}