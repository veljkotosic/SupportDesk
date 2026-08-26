using SupportDesk.Domain.Abstract.Repository;
using SupportDesk.Domain.Models.SupportAgentInvite.ValueObjects;

namespace SupportDesk.Domain.Models.SupportAgentInvite.Repository;

public interface ISupportAgentInviteRepository : IAbstractRepository<SupportAgentInvite, SupportAgentInviteId>
{
    Task<SupportAgentInvite?> GetByCodeAsync(SupportAgentInviteCode code, CancellationToken cancellationToken = default);
}