using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Common.Auth.Permissions;

namespace SupportDesk.Application.Models.SupportAgentInvites.Command.RevokeSupportAgentInvite;

public sealed record RevokeSupportAgentInviteCommand(Guid SupportAgentInviteId) : ICommand
{
    public ICollection<Permission> GetRequiredPermissions()
    {
        return [
            Permissions.SupportAgentInvites.Revoke
        ];
    }
}