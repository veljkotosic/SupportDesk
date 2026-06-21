using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Auth.AuthService;
using SupportDeskWebApi.Data.Database.UnitOfWork;
using SupportDeskWebApi.Data.Entities.SupportAgentInviteCode;
using SupportDeskWebApi.Data.Entities.SupportAgentInviteCode.Repository;
using SupportDeskWebApi.Data.Entities.User;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Auth.RegisterSupportAgent;

public class RegisterSupportAgentRequestHandler
    : IRequestHandler<RegisterSupportAgentRequest, RegisterSupportAgentResult>
{
    private readonly IAuthService _authService;
    private readonly ISupportAgentInviteCodeRepository _supportAgentInviteCodeRepository;
    private readonly ITokenProvider _tokenProvider;
    private readonly IRefreshTokenManager _refreshTokenManager;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterSupportAgentRequestHandler(
        IAuthService authService,
        ISupportAgentInviteCodeRepository supportAgentInviteCodeRepository,
        ITokenProvider tokenProvider,
        IRefreshTokenManager refreshTokenManager,
        IUnitOfWork unitOfWork)
    {
        _authService = authService;
        _supportAgentInviteCodeRepository = supportAgentInviteCodeRepository;
        _tokenProvider = tokenProvider;
        _refreshTokenManager = refreshTokenManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<RegisterSupportAgentResult> HandleAsync(RegisterSupportAgentRequest request, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(request.Code, out var parsedCode))
        {
            throw AuthException.InvalidInviteCode();
        }

        var inviteCode = await _supportAgentInviteCodeRepository.GetByCodeAsync(parsedCode, cancellationToken);
        
        if (inviteCode is null)
        {
            throw AuthException.InvalidInviteCode();
        }
        
        if (inviteCode.Email != request.Email)
        {
            throw AuthException.InvalidInviteCode();
        }
        
        if (inviteCode.ExpiresAt < DateTime.UtcNow)
        {
            inviteCode.Status = SupportAgentInviteCodeStatus.Expired;
            await _supportAgentInviteCodeRepository.SaveAsync(inviteCode, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            throw AuthException.InvalidInviteCode();
        }
        
        if (inviteCode.Status != SupportAgentInviteCodeStatus.Active)
        {
            throw AuthException.InvalidInviteCode();
        }
        
        inviteCode.Status = SupportAgentInviteCodeStatus.Used;
        inviteCode.UsedAt = DateTime.UtcNow;       
        
        await _supportAgentInviteCodeRepository.SaveAsync(inviteCode, cancellationToken);

        var role = UserRole.SupportAgent;

        var supportAgent = new Data.Entities.User.User()
        {
            Id = Guid.NewGuid(),
            Type = UserType.SupportAgent,
            OrganizationId = inviteCode.OrganizationId,
            UserName = request.Username,
            NormalizedUserName = request.Username.ToUpperInvariant(),
            Email = request.Email,
            NormalizedEmail = request.Email.ToUpperInvariant(),
            CreatedAt = DateTime.UtcNow,
        };
        
        await _authService.SignUpWithEmailAndPasswordAsync(supportAgent, request.Password, role, cancellationToken);
        
        var loginResult = await _authService.LoginWithEmailAndPasswordAsync(request.Email, request.Password, cancellationToken);
        
        var accessToken = _tokenProvider.GenerateAccessToken(loginResult.User, loginResult.Roles);
        var refreshTokenValue = _tokenProvider.GenerateRefreshToken();
        
        var refreshToken = await _refreshTokenManager.AddAsync(refreshTokenValue, loginResult.User.Id, role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return new RegisterSupportAgentResult(accessToken, refreshTokenValue, refreshToken.ExpiresAt);
    }
}
