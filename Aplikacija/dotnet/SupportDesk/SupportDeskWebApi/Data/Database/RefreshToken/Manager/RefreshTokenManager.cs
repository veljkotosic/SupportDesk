using Microsoft.EntityFrameworkCore;
using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Auth.Jwt;
using SupportDeskWebApi.Data.Entities.User;

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
    
    public async Task<RefreshToken> AddAsync(string token, Guid userId, UserRole userRole, CancellationToken cancellationToken = default)
    {
        DateTime expiresAt = DateTime.UtcNow.AddDays(7);

        if (userRole == UserRole.Customer)
        {
            expiresAt = DateTime.UtcNow.AddDays(_jwtSettings.CustomerRefreshTokenExpirationDays);
        } 
        else if (UserRole.OrganizationRoles().Contains(userRole))
        {
            expiresAt = DateTime.UtcNow.AddDays(_jwtSettings.OrganizationRefreshTokenExpirationDays);
        }
        
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            UserId = userId,
            ExpiresAt = expiresAt
        };
        
        await _dbContext.AddAsync(refreshToken, cancellationToken);
        
        return refreshToken;
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