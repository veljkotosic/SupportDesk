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
}