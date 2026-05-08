namespace SupportDeskWebApi.Data.Entities.Common.Repository;

public interface IDeleteRepository<in TEntity>
    where TEntity : IEntity
{
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
}