namespace SupportDesk.Application.Abstract.Auth.UserContext;

/// <summary>
/// Provides access to the authenticated user identity and context for the current request.
/// </summary>
public interface IUserContext
{
    /// <summary>
    /// Gets the unique identifier of the currently authenticated user.
    /// </summary>
    /// <returns>The <see cref="Guid"/> of the current user.</returns>
    /// <exception cref="System.InvalidOperationException">Thrown when no user context is available or the user is unauthenticated.</exception>
    Guid GetCurrentUserId();

    /// <summary>
    /// Gets the unique identifier of the currently authenticated user or null if the user is not authenticated.
    /// </summary>
    /// <returns>The <see cref="Guid"/> of the current user or <see langword="null"/>.</returns>
    Guid? TryGetCurrentUserId();
}