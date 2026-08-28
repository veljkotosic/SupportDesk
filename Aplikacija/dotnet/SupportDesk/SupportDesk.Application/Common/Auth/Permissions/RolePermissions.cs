using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Domain.Models.User.Enums;

namespace SupportDesk.Application.Common.Auth.Permissions;

public static class RolePermissions
{
    private static readonly Dictionary<UserRole, List<Permission>> DefaultPermissions = new()
    {
        [UserRole.Customer] =
        [

        ],
        [UserRole.OrganizationAdmin] =
        [
            Permissions.Categories.Add,
            Permissions.Categories.Update,
            Permissions.Categories.Delete
        ],
        [UserRole.SupportAgent] =
        [

        ]
    };

    public static IReadOnlyCollection<Permission> GetDefaultPermissions(UserRole role)
    {
        return DefaultPermissions.TryGetValue(role, out var permissions) ? permissions : [];
    }
        
}