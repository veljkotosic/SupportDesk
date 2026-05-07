using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Auth.AuthService;
using SupportDeskWebApi.Data.Database.UnitOfWork;
using SupportDeskWebApi.Data.Entities.SupportAgentInviteCode;
using SupportDeskWebApi.Data.Entities.SupportAgentInviteCode.Repository;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.User.OrganizationAdmin.RevokeSupportAgentInviteCode;

public class RevokeSupportAgentInviteCodeRequestHandler
    : IRequestHandler<RevokeSupportAgentInviteCodeRequest>
{
    private readonly IUserContext _userContext;
    private readonly ISupportAgentInviteCodeRepository _supportAgentInviteCodeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RevokeSupportAgentInviteCodeRequestHandler(
        IUserContext userContext,
        ISupportAgentInviteCodeRepository supportAgentInviteCodeRepository,
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _supportAgentInviteCodeRepository = supportAgentInviteCodeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task HandleAsync(RevokeSupportAgentInviteCodeRequest request, CancellationToken cancellationToken = default)
    {
        var organizationId = _userContext.GetCurrentUsersOrganizationId();
        
        var inviteCode = await _supportAgentInviteCodeRepository.GetByCodeAsync(new Guid(request.Code), cancellationToken);

        if (inviteCode is null)
        {
            throw AuthException.InvalidInviteCode();
        }
        
        if (inviteCode.OrganizationId != organizationId)
        {
            throw AuthException.InvalidInviteCode();
        }
        
        inviteCode.Status = SupportAgentInviteCodeStatus.Revoked;
        inviteCode.RevokedAt = DateTime.UtcNow;
        
        await _supportAgentInviteCodeRepository.SaveAsync(inviteCode, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}