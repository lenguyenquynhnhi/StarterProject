using DevOpsLab.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DevOpsLab.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of the readiness probe used by /healthz/ready.
/// </summary>
public sealed class EfDatabaseConnectivityProbe : IDatabaseConnectivityProbe
{
    private readonly AppDbContext _dbContext;

    public EfDatabaseConnectivityProbe(AppDbContext dbContext) => _dbContext = dbContext;

    public Task<bool> CanConnectAsync(CancellationToken cancellationToken = default)
        => _dbContext.Database.CanConnectAsync(cancellationToken);

    public async Task<bool> HasPendingMigrationsAsync(CancellationToken cancellationToken = default)
    {
        // A relational provider is required for migrations; in-memory providers
        // used by tests report no pending migrations.
        if (!_dbContext.Database.IsRelational())
            return false;

        var pending = await _dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
        return pending.Any();
    }
}
