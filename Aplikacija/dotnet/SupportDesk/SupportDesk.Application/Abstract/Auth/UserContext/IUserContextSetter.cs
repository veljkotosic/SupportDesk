namespace SupportDesk.Application.Abstract.Auth.UserContext;

/// <summary>
/// Allows non-HTTP execution pipelines (such as background workers, message consumers, and test harnesses)
/// to explicitly set the active user identity for the current dependency injection scope.
/// </summary>
public interface IUserContextSetter
{
    /// <summary>
    /// Sets the unique identifier of the user executing the current scoped operation.
    /// </summary>
    /// <param name="userId">The user identifier to set, or <see langword="null"/> if unauthenticated or anonymous.</param>
    void SetCurrentUserId(Guid? userId);
}