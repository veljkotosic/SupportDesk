using SupportDeskWebApi.Auth.Abstract;
using SupportDeskWebApi.Data.Database.UnitOfWork;
using SupportDeskWebApi.Data.Entities.SupportAgentInviteCode;
using SupportDeskWebApi.Data.Entities.SupportAgentInviteCode.Repository;
using SupportDeskWebApi.Requests.Abstract;

namespace SupportDeskWebApi.Requests.User.OrganizationAdmin.GenerateSupportAgentInviteCode;

public class GenerateSupportAgentInviteCodeRequestHandler
    : IRequestHandler<GenerateSupportAgentInviteCodeRequest, GenerateSupportAgentInviteCodeResult>
{
    private readonly IUserContext _userContext;
    private readonly ISupportAgentInviteCodeRepository _supportAgentInviteCodeRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    public GenerateSupportAgentInviteCodeRequestHandler(
        IUserContext userContext,
        ISupportAgentInviteCodeRepository supportAgentInviteCodeRepository, 
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _supportAgentInviteCodeRepository = supportAgentInviteCodeRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<GenerateSupportAgentInviteCodeResult> HandleAsync(GenerateSupportAgentInviteCodeRequest request, CancellationToken cancellationToken = default)
    {
        var organizationId = _userContext.GetCurrentUsersOrganizationId()!;
        
        var inviteCode = await _supportAgentInviteCodeRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (inviteCode is null)
        {
            inviteCode = new SupportAgentInviteCode
            {
                Id = Guid.NewGuid(),
                OrganizationId = (Guid)organizationId,
                Email = request.Email,
                Code = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(3),
                Status = SupportAgentInviteCodeStatus.Active,
                UsedAt = DateTime.MaxValue
            };
        
            await _supportAgentInviteCodeRepository.SaveAsync(inviteCode, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return new GenerateSupportAgentInviteCodeResult(inviteCode.Code.ToString());
    }
}