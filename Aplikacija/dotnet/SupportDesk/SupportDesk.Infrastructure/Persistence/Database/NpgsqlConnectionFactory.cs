using System.Data.Common;
using Npgsql;
using SupportDesk.Application.Abstract.Database;

namespace SupportDesk.Infrastructure.Persistence.Database;

public sealed class NpgsqlConnectionFactory : IDbConnectionFactory, IAsyncDisposable, IDisposable
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly bool _ownsDataSource;

    public NpgsqlConnectionFactory(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
        _ownsDataSource = false;
    }

    public NpgsqlConnectionFactory(string connectionString)
    {
        _dataSource = NpgsqlDataSource.Create(connectionString);
        _ownsDataSource = true;
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
        if (_ownsDataSource)
        {
            _dataSource.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_ownsDataSource)
        {
            await _dataSource.DisposeAsync();
        }
    }
}