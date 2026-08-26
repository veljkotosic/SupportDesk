namespace SupportDesk.Application.Abstract.Auth.Permission;

public sealed class PermissionChecker
{
    private readonly IUserContext _userContext;
    private readonly IPermissionService _permissionService;

    public PermissionChecker(IUserContext userContext, IPermissionService permissionService)
    {
        _userContext = userContext;
        _permissionService = permissionService;
    }
    
    public async Task CheckAsync(ICollection<Auth.Permission.Permission> requiredPermissions, CancellationToken cancellationToken = default)
    {
        if (requiredPermissions.Count == 0)
        {
            return;
        }

        var userId = _userContext.GetCurrentUserId();
        
        var userPermissions = await _permissionService.GetPermissionsForUserAsync(userId, cancellationToken);

        foreach (var requiredPermission in requiredPermissions)
        {
            if (!userPermissions.Contains(requiredPermission.Value, StringComparer.OrdinalIgnoreCase))
            {
                throw new PermissionException(requiredPermission);
            }
        }
    }
}