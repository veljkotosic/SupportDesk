using SupportDeskWebApi.Data.Database.RefreshToken;
using SupportDeskWebApi.Data.Entities.User;

namespace SupportDeskWebApi.Auth.Abstract;

public interface IRefreshTokenManager
{
    Task<RefreshToken> AddAsync(string token, Guid userId, UserRole userRole, CancellationToken cancellationToken = default);
    Task<RefreshToken> GetByValueAsync(string token, CancellationToken cancellationToken = default);
    Task RevokeAsync(string token, CancellationToken cancellationToken = default);
    Task RevokeAllAsync(Guid userId, CancellationToken cancellationToken = default);
}