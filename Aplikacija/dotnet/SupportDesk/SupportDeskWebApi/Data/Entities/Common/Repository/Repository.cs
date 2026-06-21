using Microsoft.EntityFrameworkCore;
using SupportDeskWebApi.Data.Database;

namespace SupportDeskWebApi.Data.Entities.Common.Repository;

public abstract class Repository<TEntity> : IRepository<TEntity>
    where TEntity : class, IEntity
{
    protected readonly SupportDeskDbContext Context;
    protected readonly DbSet<TEntity> Entities;

    protected Repository(SupportDeskDbContext context)
    {
        Context = context;
        Entities = Context.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await Entities.FindAsync([id], cancellationToken);
    }

    public async Task SaveAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var ent = await Entities.FindAsync([entity.Id], cancellationToken);
        if (ent != null)
        {
            Context.Entry(ent).CurrentValues.SetValues(entity);
        }
        else
        {
            await Entities.AddAsync(entity, cancellationToken);
        }
    }
}