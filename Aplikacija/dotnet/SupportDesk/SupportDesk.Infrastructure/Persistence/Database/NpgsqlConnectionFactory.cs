using System.Data.Common;
using Npgsql;
using SupportDesk.Application.Abstract.Database;

namespace SupportDesk.Infrastructure.Persistence.Database;

public sealed class NpgsqlConnectionFactory : IDbConnectionFactory, IAsyncDisposable, IDisposable
{
    private readonly NpgsqlDataSource _dataSource;

    public NpgsqlConnectionFactory(string connectionString)
    {
        _dataSource = NpgsqlDataSource.Create(connectionString);
    }

    public DbConnection CreateConnection()
    {
        return _dataSource.CreateConnection();
    }

    public async Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        return await _dataSource.OpenConnectionAsync(cancellationToken);
    }

    public void Dispose()
    {
        _dataSource.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _dataSource.DisposeAsync();
    }
}