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
            throw AuthException.RegistrationFailed(GetRegistrationErrors(result.Errors));
        }
        
        var roleResult = await _userManager.AddToRoleAsync(user, role.Name);

        if (!roleResult.Succeeded)
        {
            throw new Exception("Failed to create user");
        }
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

    private static IReadOnlyList<string> GetRegistrationErrors(IEnumerable<IdentityError> identityErrors)
    {
        var errors = identityErrors.ToList();
        var messages = new List<string>();

        if (errors.Any(error => error.Code == "DuplicateEmail"))
        {
            messages.Add("Email is already taken.");
        }

        if (errors.Any(error => error.Code == "DuplicateUserName"))
        {
            messages.Add("Username is already taken.");
        }

        if (errors.Any(error => error.Code == "InvalidEmail"))
        {
            messages.Add("Email is invalid.");
        }

        if (errors.Any(error => error.Code == "InvalidUserName"))
        {
            messages.Add("Username is invalid.");
        }

        if (errors.Any(error => error.Code.StartsWith("Password", StringComparison.OrdinalIgnoreCase)))
        {
            messages.Add("Password must be at least 8 characters and include uppercase, lowercase, number, and symbol.");
        }

        messages.AddRange(errors
            .Where(error => !IsKnownRegistrationError(error.Code))
            .Select(error => error.Description));

        return messages.Count > 0 ? messages : ["Could not create account."];
    }

    private static bool IsKnownRegistrationError(string code)
    {
        return code is "DuplicateEmail" or "DuplicateUserName" or "InvalidEmail" or "InvalidUserName" || code.StartsWith("Password", StringComparison.OrdinalIgnoreCase);
    }
}
