using SupportDesk.Application.Abstract.Auth.Permission;

namespace SupportDesk.Application.Common.Auth.Permissions;

public static partial class Permissions
{
    public static class Messages
    {
        public static readonly Permission Send = new("Messages.Send", "You don't have permission to send a message.");
        public static readonly Permission Get = new("Messages.Get", "You don't have permission to get a message.");
    }
}