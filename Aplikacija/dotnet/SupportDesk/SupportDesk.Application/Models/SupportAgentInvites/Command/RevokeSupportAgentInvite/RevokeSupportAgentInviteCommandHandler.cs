using SupportDesk.Application.Abstract.Auth.Permission;
using SupportDesk.Application.Abstract.Command;
using SupportDesk.Domain.Abstract;
using SupportDesk.Domain.Abstract.Validation.Rule;
using SupportDesk.Domain.Common.Validation.Rules;
using SupportDesk.Domain.Models.SupportAgentInvite;
using SupportDesk.Domain.Models.SupportAgentInvite.Repository;
using SupportDesk.Domain.Models.SupportAgentInvite.ValueObjects;

namespace SupportDesk.Application.Models.SupportAgentInvites.Command.RevokeSupportAgentInvite;

internal sealed class RevokeSupportAgentInviteCommandHandler
    : AbstractCommandHandler<RevokeSupportAgentInviteCommand, RevokeSupportAgentInviteCommandHandlerContext>
{
    private readonly TimeProvider _timeProvider;
    private readonly ISupportAgentInviteRepository _supportAgentInviteRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public RevokeSupportAgentInviteCommandHandler(
        PermissionChecker permissionChecker,
        TimeProvider timeProvider,
        ISupportAgentInviteRepository supportAgentInviteRepository,
        IUnitOfWork unitOfWork) 
        : base(permissionChecker)
    {
        _timeProvider = timeProvider;
        _supportAgentInviteRepository = supportAgentInviteRepository;
        _unitOfWork = unitOfWork;
    }

    protected override async Task<RevokeSupportAgentInviteCommandHandlerContext> PrepareAsync(RevokeSupportAgentInviteCommand command, CancellationToken cancellationToken)
    {
        var inviteId = new SupportAgentInviteId(command.SupportAgentInviteId);
        
        var invite = await _supportAgentInviteRepository.GetByIdAsync(inviteId, cancellationToken);
        
        return new RevokeSupportAgentInviteCommandHandlerContext(invite, inviteId);      
    }

    protected override async Task ExecuteAsync(RevokeSupportAgentInviteCommand command, RevokeSupportAgentInviteCommandHandlerContext context, CancellationToken cancellationToken)
    {
        var invite = context.SupportAgentInvite!;
        
        invite.Revoke(_timeProvider);
        
        await _supportAgentInviteRepository.SaveAsync(invite, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);       
    }

    public override IReadOnlyList<ICollection<IRule>> GetValidationStages(RevokeSupportAgentInviteCommandHandlerContext context)
    {
        return
        [
            [
                new DomainModelExistsRule<SupportAgentInvite, SupportAgentInviteId>(context.SupportAgentInvite, context.SupportAgentInviteId)
            ]
        ];
    }
}