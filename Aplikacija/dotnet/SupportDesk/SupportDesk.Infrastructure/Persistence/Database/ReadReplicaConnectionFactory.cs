using System.Data.Common;
using Npgsql;
using SupportDesk.Application.Abstract.Database;

namespace SupportDesk.Infrastructure.Persistence.Database;

public sealed class ReadReplicaConnectionFactory : IReadDbConnectionFactory, IAsyncDisposable, IDisposable
{
    private readonly NpgsqlDataSource _readDataSource;

    public ReadReplicaConnectionFactory(NpgsqlDataSource readDataSource)
    {
        _readDataSource = readDataSource ?? throw new ArgumentNullException(nameof(readDataSource));
    }

    public DbConnection CreateConnection()
    {
        return _readDataSource.CreateConnection();
    }

    public async Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        return await _readDataSource.OpenConnectionAsync(cancellationToken);
    }

    public void Dispose()
    {
        _readDataSource.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _readDataSource.DisposeAsync();
    }
}
