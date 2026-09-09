using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Application.Common.Auth.Permissions;

namespace SupportDesk.Application.Models.SupportAgentInvites.Command.CreateSupportAgentInvite;

public sealed record CreateSupportAgentInviteCommand(string Email)
    : ICommand<CreateSupportAgentInviteCommandResult>
{
    public ICollection<Permission> GetRequiredPermissions()
    {
        return [
            Permissions.SupportAgentInvites.Create
        ];
    }
}