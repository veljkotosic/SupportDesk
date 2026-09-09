namespace SupportDesk.Application.Abstract.Auth.Permission;

/// <summary>
/// Provides services for resolving and evaluating permissions assigned to users.
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Asynchronously retrieves all permission keys granted to a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose permissions are being queried.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> containing a list of granted permission key strings.
    /// </returns>
    Task<List<string>> GetPermissionsForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Asynchronously grants a permission to a user.
    /// If the permission is granted by default for the user's role, removes any explicit revoke override;
    /// otherwise, persists an explicit grant claim.
    /// </summary>
    /// <param name="userId">The unique identifier of the user receiving the permission.</param>
    /// <param name="permission">The permission to grant.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="System.InvalidOperationException">Thrown when the user with the specified identifier is not found.</exception>
    Task GrantPermissionAsync(Guid userId, Permission permission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously revokes a permission from a user.
    /// If the permission is granted by default for the user's role, persists an explicit revoke override;
    /// otherwise, removes any existing explicit grant claim or records a revoke claim.
    /// </summary>
    /// <param name="userId">The unique identifier of the user losing the permission.</param>
    /// <param name="permission">The permission to revoke.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="System.InvalidOperationException">Thrown when the user with the specified identifier is not found.</exception>
    Task RevokePermissionAsync(Guid userId, Permission permission, CancellationToken cancellationToken = default);
}