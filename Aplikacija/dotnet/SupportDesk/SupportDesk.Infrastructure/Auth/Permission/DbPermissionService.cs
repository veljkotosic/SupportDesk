using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Common.Auth.Permissions;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.User.Validation;
using SupportDesk.Domain.Models.User.ValueObjects;
using SupportDesk.Infrastructure.Persistence.Database;

namespace SupportDesk.Infrastructure.Auth.Permission;

public class DbPermissionService : IPermissionService
{
    private const string GrantClaimType = "granted_permission";
    private const string RevokeClaimType = "revoked_permission";

    private readonly SupportDeskDbContext _dbContext;
    
    public DbPermissionService(SupportDeskDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<List<string>> GetPermissionsForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userIdVo = new UserId(userId);
        
        var user = await _dbContext.DomainUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userIdVo, cancellationToken);

        if (user is null)
        {
            return [];
        }

        var permissions = RolePermissions.GetDefaultPermissions(user.Role)
            .Select(p => p.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var userClaims = await _dbContext.UserClaims
            .Where(c => c.UserId == userId && 
                        (c.ClaimType == GrantClaimType || c.ClaimType == RevokeClaimType) && 
                        c.ClaimValue != null)
            .Select(c => new { c.ClaimType, c.ClaimValue })
            .ToListAsync(cancellationToken);

        foreach (var claim in userClaims)
        {
            if (claim.ClaimType == GrantClaimType)
            {
                permissions.Add(claim.ClaimValue!);
            }
            else if (claim.ClaimType == RevokeClaimType)
            {
                permissions.Remove(claim.ClaimValue!);
            }
        }

        return permissions.ToList();
    }

    public async Task GrantPermissionAsync(Guid userId, Application.Abstract.Auth.Permission.Permission permission, CancellationToken cancellationToken = default)
    {
        var userIdVo = new UserId(userId);
        
        var user = await _dbContext.DomainUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userIdVo, cancellationToken);

        if (user is null)
        {
            throw new ValidationException(UserErrors.NotFound(userIdVo));
        }

        var defaultPermissions = RolePermissions.GetDefaultPermissions(user.Role);
        var isDefault = defaultPermissions.Any(p => string.Equals(p.Value, permission.Value, StringComparison.OrdinalIgnoreCase));

        var existingClaims = await _dbContext.UserClaims
            .Where(c => c.UserId == userId &&
                        (c.ClaimType == GrantClaimType || c.ClaimType == RevokeClaimType) &&
                        c.ClaimValue == permission.Value)
            .ToListAsync(cancellationToken);

        var revokedClaims = existingClaims.Where(c => c.ClaimType == RevokeClaimType).ToList();
        var grantedClaims = existingClaims.Where(c => c.ClaimType == GrantClaimType).ToList();

        if (isDefault)
        {
            if (revokedClaims.Count > 0)
            {
                _dbContext.UserClaims.RemoveRange(revokedClaims);
            }

            if (grantedClaims.Count > 0)
            {
                _dbContext.UserClaims.RemoveRange(grantedClaims);
            }
        }
        else
        {
            if (revokedClaims.Count > 0)
            {
                _dbContext.UserClaims.RemoveRange(revokedClaims);
            }

            if (grantedClaims.Count == 0)
            {
                _dbContext.UserClaims.Add(new IdentityUserClaim<Guid>
                {
                    UserId = userId,
                    ClaimType = GrantClaimType,
                    ClaimValue = permission.Value
                });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokePermissionAsync(Guid userId, Application.Abstract.Auth.Permission.Permission permission, CancellationToken cancellationToken = default)
    {
        var userIdVo = new UserId(userId);
        
        var user = await _dbContext.DomainUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userIdVo, cancellationToken);

        if (user is null)
        {
            throw new ValidationException(UserErrors.NotFound(userIdVo));
        }

        var defaultPermissions = RolePermissions.GetDefaultPermissions(user.Role);
        var isDefault = defaultPermissions.Any(p => string.Equals(p.Value, permission.Value, StringComparison.OrdinalIgnoreCase));

        var existingClaims = await _dbContext.UserClaims
            .Where(c => c.UserId == userId &&
                        (c.ClaimType == GrantClaimType || c.ClaimType == RevokeClaimType) &&
                        c.ClaimValue == permission.Value)
            .ToListAsync(cancellationToken);

        var revokedClaims = existingClaims.Where(c => c.ClaimType == RevokeClaimType).ToList();
        var grantedClaims = existingClaims.Where(c => c.ClaimType == GrantClaimType).ToList();

        if (isDefault)
        {
            if (grantedClaims.Count > 0)
            {
                _dbContext.UserClaims.RemoveRange(grantedClaims);
            }

            if (revokedClaims.Count == 0)
            {
                _dbContext.UserClaims.Add(new IdentityUserClaim<Guid>
                {
                    UserId = userId,
                    ClaimType = RevokeClaimType,
                    ClaimValue = permission.Value
                });
            }
        }
        else
        {
            if (grantedClaims.Count > 0)
            {
                _dbContext.UserClaims.RemoveRange(grantedClaims);
            }
            else if (revokedClaims.Count == 0)
            {
                _dbContext.UserClaims.Add(new IdentityUserClaim<Guid>
                {
                    UserId = userId,
                    ClaimType = RevokeClaimType,
                    ClaimValue = permission.Value
                });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}