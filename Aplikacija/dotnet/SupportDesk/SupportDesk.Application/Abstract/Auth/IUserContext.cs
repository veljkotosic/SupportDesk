namespace SupportDesk.Application.Abstract.Auth;

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
}