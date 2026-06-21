using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Database.UnitOfWork;
using SupportDeskWebApi.Data.Entities.User;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Auth.RegisterCustomer;

public class RegisterCustomerRequestHandler : IRequestHandler<RegisterCustomerRequest, RegisterCustomerResult>
{
    private readonly IAuthService _authService;
    private readonly ITokenProvider _tokenProvider;
    private readonly IRefreshTokenManager _refreshTokenManager;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCustomerRequestHandler(
        IAuthService authService, 
        ITokenProvider tokenProvider,
        IRefreshTokenManager refreshTokenManager,
        IUnitOfWork unitOfWork)
    {
        _authService = authService;
        _tokenProvider = tokenProvider;
        _refreshTokenManager = refreshTokenManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<RegisterCustomerResult> HandleAsync(RegisterCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var user = new Data.Entities.User.User
        {
            Id = Guid.NewGuid(),
            Type = UserType.Customer,
            Email = request.Email,
            NormalizedEmail = request.Email.ToUpperInvariant(),
            UserName = request.Username,
            NormalizedUserName = request.Username.ToUpperInvariant(),
            CreatedAt = DateTime.UtcNow
        };

        var role = UserRole.Customer;
        
        await _authService.SignUpWithEmailAndPasswordAsync(user, request.Password, role, cancellationToken);
        
        var loginResult = await _authService.LoginWithEmailAndPasswordAsync(request.Email, request.Password, cancellationToken);
        
        var accessToken = _tokenProvider.GenerateAccessToken(loginResult.User, loginResult.Roles);
        var refreshTokenValue = _tokenProvider.GenerateRefreshToken();
        
        var refreshToken = await _refreshTokenManager.AddAsync(refreshTokenValue, loginResult.User.Id, role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return new RegisterCustomerResult(accessToken, refreshTokenValue, refreshToken.ExpiresAt);
    }
}