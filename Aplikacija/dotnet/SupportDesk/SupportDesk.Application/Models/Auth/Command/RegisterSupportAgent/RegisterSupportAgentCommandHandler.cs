using SupportDesk.Application.Abstract.Auth;
using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Common.ValueObjects;
using SupportDesk.Domain.Models.SupportAgentInvite;
using SupportDesk.Domain.Models.SupportAgentInvite.Repository;
using SupportDesk.Domain.Models.SupportAgentInvite.Validation.Rules;
using SupportDesk.Domain.Models.SupportAgentInvite.ValueObjects;
using SupportDesk.Domain.Models.User;
using SupportDesk.Domain.Models.User.Enums;
using SupportDesk.Domain.Models.User.Repository;

namespace SupportDesk.Application.Models.Auth.Command.RegisterSupportAgent;

internal sealed class RegisterSupportAgentCommandHandler
    : AbstractCommandHandler<RegisterSupportAgentCommand, RegisterSupportAgentCommandResult, RegisterSupportAgentCommandHandlerContext>
{
    private readonly TimeProvider _timeProvider;
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;
    private readonly ISupportAgentInviteRepository _supportAgentInviteCodeRepository;
    private readonly ITokenProvider _tokenProvider;
    private readonly IRefreshTokenManager _refreshTokenManager;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterSupportAgentCommandHandler(
        PermissionChecker permissionChecker,
        TimeProvider timeProvider,
        IAuthService authService,
        IUserRepository userRepository,
        ISupportAgentInviteRepository supportAgentInviteCodeRepository,
        ITokenProvider tokenProvider,
        IRefreshTokenManager refreshTokenManager,
        IUnitOfWork unitOfWork
        ) : base(permissionChecker)
    {
        _timeProvider = timeProvider;
        _authService = authService;
        _userRepository = userRepository;
        _supportAgentInviteCodeRepository = supportAgentInviteCodeRepository;
        _tokenProvider = tokenProvider;
        _refreshTokenManager = refreshTokenManager;
        _unitOfWork = unitOfWork;
    }

    protected override async Task<RegisterSupportAgentCommandHandlerContext> PrepareAsync(RegisterSupportAgentCommand command, CancellationToken cancellationToken)
    {
        var inviteCode = new SupportAgentInviteCode(command.Code);
        var email = new Email(command.Email);

        var invite = await _supportAgentInviteCodeRepository.GetByCodeAsync(inviteCode, cancellationToken);
        
        return new RegisterSupportAgentCommandHandlerContext(invite, inviteCode, email);       
    }

    protected override async Task<RegisterSupportAgentCommandResult> ExecuteAsync(RegisterSupportAgentCommand command, RegisterSupportAgentCommandHandlerContext context, CancellationToken cancellationToken)
    {
        var invite = context.SupportAgentInvite!;
        
        invite.Use(_timeProvider);

        var supportAgent = User.Create(
            command.Email,
            command.UserName,
            invite.OrganizationId.IdValue,
            UserRole.SupportAgent,
            _timeProvider);
        
        await _authService.SignUpWithEmailAndPasswordAsync(supportAgent, command.Password, cancellationToken);
        
        var accessToken = _tokenProvider.GenerateAccessToken(supportAgent);
        var refreshTokenValue = _tokenProvider.GenerateRefreshToken();
        
        var refreshToken = await _refreshTokenManager.AddAsync(refreshTokenValue, supportAgent.Id.IdValue, supportAgent.Role, _timeProvider, cancellationToken);
        await _userRepository.SaveAsync(supportAgent, cancellationToken);
        await _supportAgentInviteCodeRepository.SaveAsync(invite, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return new RegisterSupportAgentCommandResult(accessToken, refreshToken);       
    }

    public override IReadOnlyList<ICollection<IRule>> GetValidationStages(RegisterSupportAgentCommandHandlerContext context)
    {
        return [
            [
                new DomainModelExistsRule<SupportAgentInvite, SupportAgentInviteId>(context.SupportAgentInvite, "code", context.Code.CodeValue)
            ],
            [
                new SupportAgentInviteEmailMustMatchRule(context.SupportAgentInvite, context.Email)
            ]
        ];
    }
}