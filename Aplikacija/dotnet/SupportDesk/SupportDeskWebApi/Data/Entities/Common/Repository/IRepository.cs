namespace SupportDeskWebApi.Data.Entities.Common.Repository;

public interface IRepository<TEntity>
    where TEntity : IEntity
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveAsync(TEntity entity, CancellationToken cancellationToken = default);
}