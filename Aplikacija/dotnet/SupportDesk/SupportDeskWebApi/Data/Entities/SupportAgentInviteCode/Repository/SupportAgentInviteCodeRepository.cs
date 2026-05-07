using Microsoft.EntityFrameworkCore;
using SupportDeskWebApi.Data.Database;
using SupportDeskWebApi.Data.Entities.Common.Repository;

namespace SupportDeskWebApi.Data.Entities.SupportAgentInviteCode.Repository;

public class SupportAgentInviteCodeRepository 
    : Repository<SupportAgentInviteCode>, ISupportAgentInviteCodeRepository
{
    public SupportAgentInviteCodeRepository(SupportDeskDbContext context) 
        : base(context)
    {
        
    }

    public async Task<SupportAgentInviteCode?> GetByCodeAsync(Guid code, CancellationToken cancellationToken = default)
    {
        return await Context.SupportAgentInviteCodes.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
    }

    public async Task<SupportAgentInviteCode?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await Context.SupportAgentInviteCodes.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);   
    }
}