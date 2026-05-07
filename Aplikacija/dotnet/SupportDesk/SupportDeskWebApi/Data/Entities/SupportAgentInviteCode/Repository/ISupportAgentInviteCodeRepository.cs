using SupportDeskWebApi.Data.Entities.Common.Repository;

namespace SupportDeskWebApi.Data.Entities.SupportAgentInviteCode.Repository;

public interface ISupportAgentInviteCodeRepository : IRepository<SupportAgentInviteCode>
{
    Task<SupportAgentInviteCode?> GetByCodeAsync(Guid code, CancellationToken cancellationToken = default);
    Task<SupportAgentInviteCode?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}