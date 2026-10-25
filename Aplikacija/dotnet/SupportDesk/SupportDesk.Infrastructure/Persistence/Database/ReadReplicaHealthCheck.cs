using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;
using SupportDesk.Infrastructure.DependencyInjection.Configuration;

namespace SupportDesk.Infrastructure.Persistence.Database;

public sealed class ReadReplicaHealthCheck : IHealthCheck
{
    private readonly string _connectionString;

    public ReadReplicaHealthCheck(IConfiguration configuration)
    {
        var rawConnStr = configuration.GetEnvReadConnectionString();
        var builder = new NpgsqlConnectionStringBuilder(rawConnStr)
        {
            Pooling = false,
            Timeout = 3,
            CommandTimeout = 3
        };
        _connectionString = builder.ConnectionString;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var cmd = connection.CreateCommand();
            
            cmd.CommandText = "SELECT pg_is_in_recovery();";
            
            var inRecovery = (bool?)await cmd.ExecuteScalarAsync(cancellationToken);

            return inRecovery == true
                ? HealthCheckResult.Healthy("Read replica is active in standby recovery mode.")
                : HealthCheckResult.Degraded("Read replica is responding but NOT in standby mode.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Read replica health check failed: {ex.Message}");
        }
    }
}
