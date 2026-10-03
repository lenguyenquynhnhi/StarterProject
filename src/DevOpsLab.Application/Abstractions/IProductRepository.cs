using DevOpsLab.Domain.Entities;

namespace DevOpsLab.Application.Abstractions;

/// <summary>
/// Persistence abstraction for products. Deliberately free of any EF Core type so
/// that the Application layer stays testable without a database.
/// </summary>
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Product>> ListAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountAsync(string? search, CancellationToken cancellationToken = default);

    void Add(Product product);
}
