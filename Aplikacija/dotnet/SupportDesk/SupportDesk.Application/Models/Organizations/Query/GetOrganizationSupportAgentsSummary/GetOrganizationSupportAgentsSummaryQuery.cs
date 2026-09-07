using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Common.Auth.Permissions;

namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationSupportAgentsSummary;

public sealed record GetOrganizationSupportAgentsSummaryQuery
    : IQuery<GetOrganizationSupportAgentsSummaryQueryResult>
{
    public ICollection<Permission> GetRequiredPermissions()
    {
        return [
            Permissions.Organizations.ViewSupportAgentsSummary
        ];
    }
}