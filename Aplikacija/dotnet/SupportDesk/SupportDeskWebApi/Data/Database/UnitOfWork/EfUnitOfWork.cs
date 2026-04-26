namespace SupportDeskWebApi.Data.Database.UnitOfWork;

public class EfUnitOfWork : IUnitOfWork
{
    private readonly SupportDeskDbContext _context;

    public EfUnitOfWork(SupportDeskDbContext context)
    {
        _context = context;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}