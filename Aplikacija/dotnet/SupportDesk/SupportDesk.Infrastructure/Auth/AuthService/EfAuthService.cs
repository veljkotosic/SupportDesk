using Microsoft.AspNetCore.Identity;
using SupportDesk.Application.Abstract.Auth;
using SupportDesk.Application.Common.Auth;
using SupportDesk.Domain.Abstract.Validation;
using SupportDesk.Domain.Models.User;
using SupportDesk.Domain.Models.User.Enums;
using SupportDesk.Domain.Models.User.Repository;
using SupportDesk.Domain.Models.User.Validation;
using SupportDesk.Domain.Models.User.ValueObjects;
using SupportDesk.Infrastructure.Auth.Identity;

namespace SupportDesk.Infrastructure.Auth.AuthService;

public sealed class EfAuthService : IAuthService
{
    private readonly UserManager<AppIdentityUser> _userManager;
    private readonly IUserRepository _userRepository;

    public EfAuthService(
        UserManager<AppIdentityUser> userManager,
        IUserRepository userRepository)
    {
        _userManager = userManager;
        _userRepository = userRepository;
    }
    
    public async Task SignUpWithEmailAndPasswordAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        var identityUser = new AppIdentityUser
        {
            Id = user.Id.IdValue,
            UserName = user.UserName.UserNameValue,
            Email = user.Email.EmailValue
        };
        
        var createResult = await _userManager.CreateAsync(identityUser, password);
        
        if (!createResult.Succeeded)
        {
            var validationErrors = createResult.Errors.Select(e => new ValidationError(e.Code, e.Description)).ToList();
            throw new ValidationException(validationErrors);
        }
    }

    public async Task<User> LoginWithEmailAndPasswordAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var identityUser = await _userManager.FindByEmailAsync(email);

        if (identityUser is null)
        {
            throw new ValidationException(UserErrors.InvalidCredentials());
        }
        
        var isPasswordValid = await _userManager.CheckPasswordAsync(identityUser, password);

        if (!isPasswordValid)
        {
            throw new ValidationException(UserErrors.InvalidCredentials());
        }
        
        var domainUser = await _userRepository.GetByIdAsync(new UserId(identityUser.Id), cancellationToken);

        if (domainUser is null)
        {
            throw new ValidationException(UserErrors.InvalidCredentials());
        }

        return domainUser;
    }

    public async Task<User> LoginWithRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        var entity = await _userManager.FindByIdAsync(refreshToken.UserId.ToString());

        if (entity is null)
        {
            throw new ValidationException(UserErrors.InvalidCredentials()); 
        }
        
        var domainUser = await _userRepository.GetByIdAsync(new UserId(entity.Id), cancellationToken);
        
        if (domainUser is null)
        {
            throw new ValidationException(UserErrors.InvalidCredentials());
        }

        return domainUser;
    }
}