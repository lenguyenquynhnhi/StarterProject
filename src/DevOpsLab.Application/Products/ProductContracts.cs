using DevOpsLab.Domain.Entities;

namespace DevOpsLab.Application.Products;

public sealed record CreateProductRequest(Guid CategoryId, string Sku, string Name, decimal Price, int StockQuantity);

public sealed record ChangePriceRequest(decimal NewPrice);

public sealed record ReserveStockRequest(int Quantity);

public sealed record ProductResponse(
    Guid Id,
    Guid CategoryId,
    string Sku,
    string Name,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    DateTime CreatedAtUtc)
{
    public static ProductResponse FromEntity(Product product) => new(
        product.Id,
        product.CategoryId,
        product.Sku,
        product.Name,
        product.Price,
        product.StockQuantity,
        product.IsActive,
        product.CreatedAtUtc);
}
