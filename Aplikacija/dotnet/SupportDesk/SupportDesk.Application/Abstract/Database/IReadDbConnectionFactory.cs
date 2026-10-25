using System.Data.Common;

namespace SupportDesk.Application.Abstract.Database;

/// <summary>
/// CQRS Read Channel: Provides read-only database connections routed to Read Replica(s).
/// Any write/mutation attempted through this factory will fail at the database level.
/// </summary>
public interface IReadDbConnectionFactory
{
    DbConnection CreateConnection();
    Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}
