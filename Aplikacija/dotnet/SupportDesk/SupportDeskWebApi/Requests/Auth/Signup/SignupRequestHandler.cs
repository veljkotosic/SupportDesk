using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Database.UnitOfWork;
using SupportDeskWebApi.Data.Entities.User;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Auth.Signup;

public class SignupRequestHandler : IRequestHandler<SignupRequest, SignupResult>
{
    private readonly IAuthService _authService;
    private readonly ITokenProvider _tokenProvider;
    private readonly IRefreshTokenManager _refreshTokenManager;
    private readonly IUnitOfWork _unitOfWork;


    public SignupRequestHandler(
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

    public async Task<SignupResult> HandleAsync(SignupRequest request, CancellationToken cancellationToken = default)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            NormalizedEmail = request.Email.ToUpperInvariant(),
            UserName = request.Username,
            NormalizedUserName = request.Username.ToUpperInvariant(),
            CreatedAt = DateTime.UtcNow
        };
        
        var role = UserRole.FromString(request.Role);
        
        await _authService.SignUpWithEmailAndPasswordAsync(user, request.Password, role!, cancellationToken);
        
        var loginResult = await _authService.LoginWithEmailAndPasswordAsync(request.Email, request.Password, cancellationToken);
        
        var accessToken = _tokenProvider.GenerateAccessToken(loginResult.User, loginResult.Roles);
        var refreshToken = _tokenProvider.GenerateRefreshToken();
        
        await _refreshTokenManager.AddAsync(refreshToken, loginResult.User.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return new SignupResult(accessToken, refreshToken);
    }
}