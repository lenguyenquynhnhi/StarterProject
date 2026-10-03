using DevOpsLab.Application.Abstractions;
using DevOpsLab.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevOpsLab.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly AppDbContext _dbContext;

    public ProductRepository(AppDbContext dbContext) => _dbContext = dbContext;

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
        => _dbContext.Products.FirstOrDefaultAsync(p => p.Sku == sku, cancellationToken);

    public async Task<IReadOnlyList<Product>> ListAsync(
        string? search, int page, int pageSize, CancellationToken cancellationToken = default)
        => await Filter(search)
            .OrderBy(p => p.Sku)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(string? search, CancellationToken cancellationToken = default)
        => Filter(search).CountAsync(cancellationToken);

    public void Add(Product product) => _dbContext.Products.Add(product);

    private IQueryable<Product> Filter(string? search)
    {
        var query = _dbContext.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p => EF.Functions.Like(p.Name, $"%{term}%")
                                     || EF.Functions.Like(p.Sku, $"%{term}%"));
        }

        return query;
    }
}
