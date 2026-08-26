using SupportDesk.Application.Abstract.Auth.Permission;

namespace SupportDesk.Application.Abstract;

/// <summary>
/// Defines the contract for an application use case (e.g., commands and queries).
/// </summary>
/// <remarks>
/// Serves as the base interface for operations in the application layer, 
/// providing common execution requirements such as permission authorization.
/// </remarks>
public interface IUseCase
{
    /// <summary>
    /// Gets the collection of permissions required to execute this use case.
    /// </summary>
    /// <returns>
    /// An <see cref="ICollection{T}"/> of required <see cref="Permission"/> instances. 
    /// Defaults to an empty collection if no specific permissions are required.
    /// </returns>
    ICollection<Permission> GetRequiredPermissions() => [];
}