using SupportDesk.Application.Abstract.Auth.Permission;

namespace SupportDesk.Application.Common.Auth.Permissions;

public static partial class Permissions
{
    public static class Categories
    {
        public static readonly Permission Add = new("Categories.Add", "You don't have permission to add a category.");
        public static readonly Permission Update = new("Categories.Update", "You don't have permission to update a category.");
        public static readonly Permission Delete = new("Categories.Delete", "You don't have permission to delete a category.");
    }
}