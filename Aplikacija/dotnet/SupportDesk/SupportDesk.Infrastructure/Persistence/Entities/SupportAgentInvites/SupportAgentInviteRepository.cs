using Microsoft.EntityFrameworkCore;
using SupportDesk.Application.Abstract.Event;
using SupportDesk.Domain.Models.SupportAgentInvite;
using SupportDesk.Domain.Models.SupportAgentInvite.Repository;
using SupportDesk.Domain.Models.SupportAgentInvite.ValueObjects;
using SupportDesk.Infrastructure.Persistence.Abstract;
using SupportDesk.Infrastructure.Persistence.Database;

namespace SupportDesk.Infrastructure.Persistence.Entities.SupportAgentInvites;

public sealed class SupportAgentInviteRepository
    : AbstractRepository<SupportAgentInvite, SupportAgentInviteId>, ISupportAgentInviteRepository
{
    public SupportAgentInviteRepository(
        SupportDeskDbContext context,
        IDomainEventCollector domainEventCollector) 
        : base(context, domainEventCollector)
    {
        
    }

    public async Task<SupportAgentInvite?> GetByCodeAsync(SupportAgentInviteCode code, CancellationToken cancellationToken = default)
    {
        return await Context.SupportAgentInvites.FirstOrDefaultAsync(s => s.Code == code, cancellationToken);
    }
}