using Microsoft.EntityFrameworkCore;
using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Auth.Jwt;

namespace SupportDeskWebApi.Data.Database.RefreshToken.Manager;

public class RefreshTokenManager : IRefreshTokenManager
{
    private readonly SupportDeskDbContext _dbContext;
    private readonly JwtSettings _jwtSettings;

    public RefreshTokenManager(SupportDeskDbContext dbContext, JwtSettings jwtSettings)
    {
        _dbContext = dbContext;
        _jwtSettings = jwtSettings;
    }
    
    public async Task AddAsync(string token, Guid userId, CancellationToken cancellationToken = default)
    {
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.CustomerRefreshTokenExpirationDays)
        };
        
        await _dbContext.AddAsync(refreshToken, cancellationToken);
    }

    public async Task<RefreshToken> GetByValueAsync(string token, CancellationToken cancellationToken = default)
    {
        var refreshToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == token, cancellationToken);
        
        if (refreshToken is null)
        {
            throw RefreshTokenException.InvalidToken();
        }

        if (refreshToken.ExpiresAt < DateTime.UtcNow)
        {
            throw RefreshTokenException.TokenExpired();
        }
        
        return refreshToken;
    }

    public async Task RevokeAsync(string token, CancellationToken cancellationToken = default)
    {
        await _dbContext.RefreshTokens
            .Where(x => x.Token == token)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task RevokeAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await _dbContext.RefreshTokens
            .Where(x => x.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}