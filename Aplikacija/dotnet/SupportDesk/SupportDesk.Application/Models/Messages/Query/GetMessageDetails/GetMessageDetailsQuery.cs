using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Query;
using SupportDesk.Application.Common.Auth.Permissions;

namespace SupportDesk.Application.Models.Messages.Query.GetMessageDetails;

public sealed record GetMessageDetailsQuery(Guid MessageId) : IQuery<GetMessageDetailsQueryResult>
{
    public ICollection<Permission> GetRequiredPermissions()
    {
        return [
            Permissions.Messages.Get
        ];
    }
}