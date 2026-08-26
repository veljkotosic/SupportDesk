using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Domain.Models.User.Enums;

namespace SupportDesk.Application.Common.Permissions;

public static class RolePermissions
{
    private static readonly Dictionary<UserRole, List<Permission>> DefaultPermissions = new()
    {
        [UserRole.Customer] =
        [

        ],
        [UserRole.OrganizationAdmin] =
        [

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