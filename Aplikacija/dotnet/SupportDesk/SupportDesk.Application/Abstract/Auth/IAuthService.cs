using SupportDesk.Application.Common.Auth;
using SupportDesk.Domain.Models.User;

namespace SupportDesk.Application.Abstract.Auth;

/// <summary>
/// Defines authentication services for user registration and credential validation.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Asynchronously registers a new user with the specified credentials and password.
    /// </summary>
    /// <param name="user">The user entity to create.</param>
    /// <param name="password">The plain-text password to hash and associate with the user account.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task SignUpWithEmailAndPasswordAsync(User user, string password, CancellationToken cancellationToken = default);
   
    /// <summary>
    /// Asynchronously validates user credentials using email and password.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="password">The plain-text password to verify.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> containing the authenticated <see cref="User"/> instance upon successful verification.
    /// </returns>
    Task<User> LoginWithEmailAndPasswordAsync(string email, string password, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Asynchronously validates an existing refresh token and retrieves the associated user.
    /// </summary>
    /// <param name="refreshToken">The refresh token entity to validate.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Task{TResult}"/> containing the authenticated <see cref="User"/> instance.
    /// </returns>
    Task<User> LoginWithRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
}