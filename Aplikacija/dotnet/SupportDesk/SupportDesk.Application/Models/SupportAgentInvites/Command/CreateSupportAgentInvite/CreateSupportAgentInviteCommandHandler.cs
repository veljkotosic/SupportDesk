using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Auth.TenantContext;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Common.ValueObjects;
using SupportDesk.Domain.Models.SupportAgentInvite;
using SupportDesk.Domain.Models.SupportAgentInvite.Repository;
using SupportDesk.Domain.Models.SupportAgentInvite.Validation.Rules;
using SupportDesk.Domain.Models.User.Repository;

namespace SupportDesk.Application.Models.SupportAgentInvites.Command.CreateSupportAgentInvite;

internal sealed class CreateSupportAgentInviteCommandHandler
    : AbstractCommandHandler<CreateSupportAgentInviteCommand, CreateSupportAgentInviteCommandResult, CreateSupportAgentInviteCommandHandlerContext>
{
    private readonly TimeProvider _timeProvider;
    private readonly ITenantContext _tenantContext;
    private readonly IUserRepository _userRepository;
    private readonly ISupportAgentInviteRepository _supportAgentInviteRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public CreateSupportAgentInviteCommandHandler(
        PermissionChecker permissionChecker,
        TimeProvider timeProvider,
        ITenantContext tenantContext,
        IUserRepository userRepository,
        ISupportAgentInviteRepository supportAgentInviteRepository,
        IUnitOfWork unitOfWork) 
        : base(permissionChecker)
    {
        _timeProvider = timeProvider;
        _tenantContext = tenantContext;
        _userRepository = userRepository;
        _supportAgentInviteRepository = supportAgentInviteRepository;
        _unitOfWork = unitOfWork;
    }

    protected override async Task<CreateSupportAgentInviteCommandHandlerContext> PrepareAsync(CreateSupportAgentInviteCommand command, CancellationToken cancellationToken)
    {
        var email = new Email(command.Email);

        var existingUser = await _userRepository.GetByEmailAsync(email, cancellationToken);
        
        return new CreateSupportAgentInviteCommandHandlerContext(existingUser, email);      
    }

    protected override async Task<CreateSupportAgentInviteCommandResult> ExecuteAsync(CreateSupportAgentInviteCommand command, CreateSupportAgentInviteCommandHandlerContext context, CancellationToken cancellationToken)
    {
        var organizationId = (Guid)_tenantContext.GetCurrentOrganizationId()!;
        var email = context.Email!;
        
        var existingInvites = await _supportAgentInviteRepository.GetActiveInvitesByEmailAsync(email, cancellationToken);

        foreach (var existingInvite in existingInvites)
        {
            existingInvite.Revoke(_timeProvider);
            await _supportAgentInviteRepository.SaveAsync(existingInvite, cancellationToken);
        }
        
        var newInvite = SupportAgentInvite.Create(command.Email, organizationId, _timeProvider);

        await _supportAgentInviteRepository.SaveAsync(newInvite, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return new CreateSupportAgentInviteCommandResult(newInvite.Code.CodeValue.ToString());      
    }

    public override IReadOnlyList<ICollection<IRule>> GetValidationStages(CreateSupportAgentInviteCommandHandlerContext context)
    {
        return
        [
            [
                new SupportAgentInviteCannotBeCreatedForExistingUserRule(context.ExistingUser)
            ]
        ];
    }
}