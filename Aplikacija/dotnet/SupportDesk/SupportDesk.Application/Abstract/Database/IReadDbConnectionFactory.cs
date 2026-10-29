using System.Data.Common;

namespace SupportDesk.Application.Abstract.Database;

public interface IReadDbConnectionFactory
{
    DbConnection CreateConnection();
    Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}
