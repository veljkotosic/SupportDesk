using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Auth.AuthService;
using SupportDeskWebApi.Data.Database.UnitOfWork;
using SupportDeskWebApi.Data.Entities.Organization;
using SupportDeskWebApi.Data.Entities.Organization.Repository;
using SupportDeskWebApi.Data.Entities.User;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.Auth.RegisterOrganization;

public class RegisterOrganizationRequestHandler 
    : IRequestHandler<RegisterOrganizationRequest, RegisterOrganizationResult>
{
    private readonly IAuthService _authService;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly ITokenProvider _tokenProvider;
    private readonly IRefreshTokenManager _refreshTokenManager;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterOrganizationRequestHandler(
        IAuthService authService,
        IOrganizationRepository organizationRepository,
        ITokenProvider tokenProvider,
        IRefreshTokenManager refreshTokenManager,
        IUnitOfWork unitOfWork)
    {
        _authService = authService;
        _organizationRepository = organizationRepository;
        _tokenProvider = tokenProvider;
        _refreshTokenManager = refreshTokenManager;
        _unitOfWork = unitOfWork;
    }

    public async Task<RegisterOrganizationResult> HandleAsync(RegisterOrganizationRequest request, CancellationToken cancellationToken = default)
    {
        var existingOrganization = await _organizationRepository.GetByNameAsync(request.OrganizationName, cancellationToken);

        if (existingOrganization is not null)
        {
            throw AuthException.OrganizationAlreadyExist();
        }
        
        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Status = OrganizationStatus.Active,
            Name = request.OrganizationName,
            CreatedAt = DateTime.UtcNow
        };
        
        await _organizationRepository.SaveAsync(organization, cancellationToken);
        
        var organizationAdmin = new Data.Entities.User.User
        {
            Id = Guid.NewGuid(),
            OrganizationId = organization.Id,
            UserName = request.Username,
            NormalizedUserName = request.Username.ToUpperInvariant(),
            Email = request.Email,
            NormalizedEmail = request.Email.ToUpperInvariant(),
            CreatedAt = organization.CreatedAt
        };
        
        var role = UserRole.OrganizationAdmin;
        
        await _authService.SignUpWithEmailAndPasswordAsync(organizationAdmin, request.Password, role, cancellationToken);
        
        var loginResult = await _authService.LoginWithEmailAndPasswordAsync(request.Email, request.Password, cancellationToken);
        
        var accessToken = _tokenProvider.GenerateAccessToken(loginResult.User, loginResult.Roles);
        var refreshTokenValue = _tokenProvider.GenerateRefreshToken();
        
        var refreshToken = await _refreshTokenManager.AddAsync(refreshTokenValue, loginResult.User.Id, role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return new RegisterOrganizationResult(accessToken, refreshTokenValue, refreshToken.ExpiresAt);
    }
}