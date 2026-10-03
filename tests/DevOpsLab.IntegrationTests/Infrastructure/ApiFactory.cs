using DevOpsLab.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace DevOpsLab.IntegrationTests.Infrastructure;

/// <summary>
/// Boots the real API pipeline in memory.
///
/// TODO (Session 4): replace the in-memory provider below with Testcontainers so
/// that integration tests run against a real SQL Server, identically on a
/// developer laptop and on the CI runner. The in-memory provider does not enforce
/// relational constraints and therefore hides an entire class of defects.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"devopslab-tests-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Production);

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();

            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(_databaseName));

            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();

            dbContext.Database.EnsureCreated();
            DatabaseSeeder.SeedAsync(dbContext, loggerFactory.CreateLogger("TestSeed")).GetAwaiter().GetResult();
        });
    }
}
