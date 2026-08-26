using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Common.Permissions;
using SupportDesk.Domain.Models.User.ValueObjects;
using SupportDesk.Infrastructure.Persistence.Database;

namespace SupportDesk.Infrastructure.Auth.Permission;

public class DbPermissionService : IPermissionService
{
    public const string GrantClaimType = "granted_permission";
    public const string RevokeClaimType = "revoked_permission";

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
}