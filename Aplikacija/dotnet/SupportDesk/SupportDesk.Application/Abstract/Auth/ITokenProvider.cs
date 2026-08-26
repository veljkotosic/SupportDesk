using SupportDesk.Application.Common.Auth;
using SupportDesk.Domain.Models.User;

namespace SupportDesk.Application.Abstract.Auth;

/// <summary>
/// Provides methods for generating authentication security tokens (access tokens and refresh tokens).
/// </summary>
public interface ITokenProvider
{
    /// <summary>
    /// Generates a signed access token for the specified user containing their claims and roles.
    /// </summary>
    /// <param name="user">The user entity for whom the access token is generated.</param>
    /// <returns>A new <see cref="AccessToken"/> instance.</returns>
    AccessToken GenerateAccessToken(User user);
    
    /// <summary>
    /// Generates a cryptographically secure random string to be used as a refresh token.
    /// </summary>
    /// <returns>A secure random string representing the refresh token value.</returns>
    string GenerateRefreshToken();
}