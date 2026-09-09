using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Common.Auth.Permissions;

namespace SupportDesk.Application.Models.Organizations.Query.GetOrganizationTemplateAnswers;

public sealed record GetOrganizationTemplateAnswersQuery : IQuery<GetOrganizationTemplateAnswersQueryResult>
{
    public ICollection<Permission> GetRequiredPermissions()
    {
        return [
            Permissions.TemplateAnswers.Get
        ];
    }
}