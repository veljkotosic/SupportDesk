using SupportDesk.Application.Abstract.Auth.Permission;

namespace SupportDesk.Application.Common.Auth.Permissions;

public static partial class Permissions
{
    public static class Faqs
    {
        public static readonly Permission Add = new("Faqs.Add", "You don't have permission to add a FAQ.");
        public static readonly Permission Update = new("Faqs.Update", "You don't have permission to update a FAQ.");
        public static readonly Permission Delete = new("Faqs.Delete", "You don't have permission to delete a FAQ.");
    }
}