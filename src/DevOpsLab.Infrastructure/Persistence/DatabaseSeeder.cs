using DevOpsLab.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DevOpsLab.Infrastructure.Persistence;

/// <summary>
/// Idempotent seed data. Running it twice must not create duplicates — the same
/// property that safe database migrations require from Session 10 onwards.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext dbContext, ILogger logger, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Categories.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Seed skipped: catalog already contains data.");
            return;
        }

        var laptops = Category.Create("Laptops");
        var peripherals = Category.Create("Peripherals");
        var displays = Category.Create("Displays");

        dbContext.Categories.AddRange(laptops, peripherals, displays);

        dbContext.Products.AddRange(
            Product.Create(laptops.Id, "LAP-14-PRO", "14-inch Pro Laptop", 1899.00m, 25),
            Product.Create(laptops.Id, "LAP-16-STD", "16-inch Standard Laptop", 1299.00m, 40),
            Product.Create(peripherals.Id, "KB-MECH-87", "87-key Mechanical Keyboard", 129.50m, 120),
            Product.Create(peripherals.Id, "MOU-ERG-01", "Ergonomic Wireless Mouse", 79.90m, 200),
            Product.Create(displays.Id, "MON-27-4K", "27-inch 4K Monitor", 549.00m, 60));

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seed completed: 3 categories and 5 products inserted.");
    }
}
