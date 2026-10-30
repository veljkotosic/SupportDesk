using Microsoft.Extensions.Diagnostics.HealthChecks;
using SupportDesk.Application.Abstract.Database;

namespace SupportDesk.Infrastructure.Persistence.Database;

public sealed class ReadReplicaHealthCheck : IHealthCheck
{
    private readonly IReadDbConnectionFactory _connectionFactory;

    public ReadReplicaHealthCheck(IReadDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
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
