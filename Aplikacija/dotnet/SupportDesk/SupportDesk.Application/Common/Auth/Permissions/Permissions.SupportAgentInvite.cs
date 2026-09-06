using SupportDesk.Application.Abstract.Auth.Permission;

namespace SupportDesk.Application.Common.Auth.Permissions;

public static partial class Permissions
{
    public static class SupportAgentInvites
    {
        public static readonly Permission Create = new("SupportAgentInvites.Create", "You don't have permission to create a support agent invite.");      
        public static readonly Permission Revoke = new("SupportAgentInvites.Revoke", "You don't have permission to revoke a support agent invite.");     
    }
}