using Microsoft.AspNetCore.Identity;
using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Database.RefreshToken;
using SupportDeskWebApi.Data.Entities.User;

namespace SupportDeskWebApi.Auth.AuthService;

public class EfAuthService : IAuthService
{
    private readonly UserManager<User> _userManager;

    public EfAuthService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task SignUpWithEmailAndPasswordAsync(User user, string password, UserRole role, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            throw new Exception("Failed to create user");
        }
        
        await _userManager.AddToRoleAsync(user, role.Name);
    }

    public async Task<LoginResult> LoginWithEmailAndPasswordAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            throw AuthException.InvalidCredentials();
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);

        if (!isPasswordValid)
        {
            throw AuthException.InvalidCredentials();
        }

        var roles = new List<UserRole>();
        var rolesNames = await _userManager.GetRolesAsync(user);

        foreach (var roleName in rolesNames)
        {
            var role = UserRole.FromString(roleName);
            
            if (role is null) throw new Exception($"Role {roleName} not found");
            
            roles.Add(role);
        }
        
        return new LoginResult(user, roles);
    }

    public async Task<LoginResult> LoginWithRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(refreshToken.UserId.ToString());

        if (user is null)
        {
            throw AuthException.InvalidCredentials();
        }

        var roles = new List<UserRole>();
        var rolesNames = await _userManager.GetRolesAsync(user);

        foreach (var roleName in rolesNames)
        {
            var role = UserRole.FromString(roleName);
            
            if (role is null) throw new Exception($"Role {roleName} not found");
            
            roles.Add(role);
        }
        
        return new LoginResult(user, roles);
    }
}