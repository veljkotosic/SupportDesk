using SupportDesk.Application.Abstract.Auth;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Models.Organization;
using SupportDesk.Domain.Models.Organization.Repository;
using SupportDesk.Domain.Models.Organization.Validation.Rules;
using SupportDesk.Domain.Models.Organization.ValueObjects;
using SupportDesk.Domain.Models.User;
using SupportDesk.Domain.Models.User.Enums;
using SupportDesk.Domain.Models.User.Repository;

namespace SupportDesk.Application.Models.Auth.Command.RegisterOrganization;

public sealed class RegisterOrganizationCommandHandler
    : AbstractCommandHandler<RegisterOrganizationCommand, RegisterOrganizationCommandResult, RegisterOrganizationCommandHandlerContext>
{
    private readonly IAuthService _authService;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITokenProvider _tokenProvider;
    private readonly IRefreshTokenManager _refreshTokenManager;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterOrganizationCommandHandler(
        PermissionChecker permissionChecker,
        IAuthService authService,
        IOrganizationRepository organizationRepository,
        IUserRepository userRepository,
        ITokenProvider tokenProvider,
        IRefreshTokenManager refreshTokenManager,
        IUnitOfWork unitOfWork
        ) : base(permissionChecker)
    {
        _authService = authService;
        _organizationRepository = organizationRepository;
        _tokenProvider = tokenProvider;
        _refreshTokenManager = refreshTokenManager;
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
    }

    protected override async Task<RegisterOrganizationCommandHandlerContext> PrepareAsync(RegisterOrganizationCommand command, CancellationToken cancellationToken)
    {
        var organizationName = new OrganizationName(command.OrganizationName);
        
        var existingOrganization = await _organizationRepository.GetByNameAsync(organizationName, cancellationToken);

        return new RegisterOrganizationCommandHandlerContext(organizationName, existingOrganization);
    }

    protected override async Task<RegisterOrganizationCommandResult> ExecuteAsync(RegisterOrganizationCommand command, RegisterOrganizationCommandHandlerContext context, CancellationToken cancellationToken)
    {
        var organization = Organization.Create(command.OrganizationName);

        var organizationAdmin = User.Create(command.Email, command.Username, organization.Id.IdValue, UserRole.OrganizationAdmin);
        
        await _authService.SignUpWithEmailAndPasswordAsync(organizationAdmin, command.Password, cancellationToken);
        
        var accessToken = _tokenProvider.GenerateAccessToken(organizationAdmin);
        var refreshTokenValue = _tokenProvider.GenerateRefreshToken();

        var refreshToken = await _refreshTokenManager.AddAsync(refreshTokenValue, organizationAdmin.Id.IdValue, organizationAdmin.Role, cancellationToken);

        await _organizationRepository.SaveAsync(organization, cancellationToken);
        await _userRepository.SaveAsync(organizationAdmin, cancellationToken);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);       
        
        return new RegisterOrganizationCommandResult(accessToken, refreshToken);
    }

    public override IReadOnlyList<ICollection<IRule>> GetValidationStages(RegisterOrganizationCommandHandlerContext context)
    {
        return [
            [
                new CannotCreateOrganizationWithExistingNameRule(context.OrganizationName, context.ExistingOrganization)
            ]
        ];
    }
}