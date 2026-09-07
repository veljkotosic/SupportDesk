using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Common.Auth.Permissions;

namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationKnowledgeBase;

public sealed record GetOrganizationKnowledgeBaseQuery : IQuery<GetOrganizationKnowledgeBaseQueryResult>
{
    public ICollection<Permission> GetRequiredPermissions()
    {
        return [
            Permissions.Organizations.ViewKnowledgeBase
        ];
    }
}