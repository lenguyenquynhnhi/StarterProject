using DevOpsLab.Application.Abstractions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DevOpsLab.WebApi.HealthChecks;

/// <summary>
/// Readiness check. The service is only "ready" when the database is reachable
/// AND every migration has been applied. This is what a Blue-Green traffic switch
/// must wait for — a process that is merely alive is not ready to serve traffic.
/// </summary>
public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly IDatabaseConnectivityProbe _probe;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(IDatabaseConnectivityProbe probe, ILogger<DatabaseHealthCheck> logger)
    {
        _probe = probe;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await _probe.CanConnectAsync(cancellationToken))
                return HealthCheckResult.Unhealthy("Database is not reachable.");

            if (await _probe.HasPendingMigrationsAsync(cancellationToken))
                return HealthCheckResult.Unhealthy("Database has pending migrations.");

            return HealthCheckResult.Healthy("Database is reachable and up to date.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Readiness probe failed while checking the database.");
            return HealthCheckResult.Unhealthy("Readiness probe threw an exception.", ex);
        }
    }
}
