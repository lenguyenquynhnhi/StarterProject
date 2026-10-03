using DevOpsLab.Application.Abstractions;
using DevOpsLab.Domain.Entities;

namespace DevOpsLab.UnitTests.Fakes;

/// <summary>
/// Hand-written in-memory fakes. They keep the unit test suite free of a mocking
/// framework and make the behaviour of each test obvious at a glance.
/// </summary>
public sealed class FakeProductRepository : IProductRepository
{
    private readonly List<Product> _products = new();

    public IReadOnlyList<Product> Items => _products;

    public void Seed(params Product[] products) => _products.AddRange(products);

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_products.FirstOrDefault(p => p.Id == id));

    public Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default)
        => Task.FromResult(_products.FirstOrDefault(p => p.Sku == sku));

    public Task<IReadOnlyList<Product>> ListAsync(
        string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Product> result = Filter(search)
            .OrderBy(p => p.Sku)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(result);
    }

    public Task<int> CountAsync(string? search, CancellationToken cancellationToken = default)
        => Task.FromResult(Filter(search).Count());

    public void Add(Product product) => _products.Add(product);

    private IEnumerable<Product> Filter(string? search)
        => string.IsNullOrWhiteSpace(search)
            ? _products
            : _products.Where(p =>
                p.Name.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase) ||
                p.Sku.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase));
}

public sealed class FakeCategoryRepository : ICategoryRepository
{
    private readonly HashSet<Guid> _ids = new();

    public void Seed(params Guid[] ids)
    {
        foreach (var id in ids) _ids.Add(id);
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_ids.Contains(id));
}

public sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        return Task.FromResult(1);
    }
}
