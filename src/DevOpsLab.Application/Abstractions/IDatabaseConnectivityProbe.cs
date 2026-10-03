namespace DevOpsLab.Application.Abstractions;

/// <summary>
/// Readiness probe for the backing store. Kept as an abstraction so the readiness
/// health check can be unit-tested without a real database, and so the WebApi
/// layer does not take a direct dependency on the ORM.
/// </summary>
public interface IDatabaseConnectivityProbe
{
    /// <summary>True when a connection to the database can be opened.</summary>
    Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>True when migrations exist that have not been applied yet.</summary>
    Task<bool> HasPendingMigrationsAsync(CancellationToken cancellationToken = default);
}
