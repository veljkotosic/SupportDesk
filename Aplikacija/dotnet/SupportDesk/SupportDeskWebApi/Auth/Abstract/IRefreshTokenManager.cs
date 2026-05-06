using SupportDeskWebApi.Data.Database.RefreshToken;

namespace SupportDeskWebApi.Auth.Abstract;

public interface IRefreshTokenManager
{
    Task AddAsync(string token, Guid userId, CancellationToken cancellationToken = default);
    Task<RefreshToken> GetByValueAsync(string token, CancellationToken cancellationToken = default);
    Task RevokeAsync(string token, CancellationToken cancellationToken = default);
    Task RevokeAllAsync(Guid userId, CancellationToken cancellationToken = default);
}