using SupportDesk.Application.Common.Auth;
using SupportDesk.Domain.Models.User.Enums;

namespace SupportDesk.Application.Abstract.Auth;

/// <summary>
/// Defines operations for managing the lifecycle, storage, and revocation of refresh tokens.
/// </summary>
public interface IRefreshTokenManager
{
    /// <summary>
    /// Asynchronously stores a newly generated refresh token for a user.
    /// </summary>
    /// <param name="token">The refresh token string value.</param>
    /// <param name="userId">The unique identifier of the user to whom the token is issued.</param>
    /// <param name="userRole">The role assigned to the user for the token session.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> containing the created <see cref="RefreshToken"/> instance.
    /// </returns>
    Task<RefreshToken> AddAsync(string token, Guid userId, UserRole userRole, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Asynchronously retrieves a refresh token record by its string value.
    /// </summary>
    /// <param name="token">The refresh token string value to look up.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> containing the matching <see cref="RefreshToken"/> instance.
    /// </returns>
    Task<RefreshToken> GetByValueAsync(string token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Asynchronously revokes a specific refresh token by its string value.
    /// </summary>
    /// <param name="token">The refresh token string value to revoke.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous revocation operation.</returns>
    Task RevokeAsync(string token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Asynchronously revokes all active refresh tokens associated with a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose refresh tokens will be revoked.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous revocation operation.</returns>
    Task RevokeAllAsync(Guid userId, CancellationToken cancellationToken = default);
}