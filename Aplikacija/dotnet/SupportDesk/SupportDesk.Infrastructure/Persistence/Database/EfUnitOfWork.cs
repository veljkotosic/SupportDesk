using SupportDesk.Domain.Abstract;

namespace SupportDesk.Infrastructure.Persistence.Database;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly SupportDeskDbContext _dbContext;

    public EfUnitOfWork(SupportDeskDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}