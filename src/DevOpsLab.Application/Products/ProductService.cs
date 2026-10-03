using DevOpsLab.Application.Abstractions;
using DevOpsLab.Application.Common;
using DevOpsLab.Domain.Common;
using DevOpsLab.Domain.Entities;

namespace DevOpsLab.Application.Products;

public sealed class ProductService
{
    public const int MaxPageSize = 100;

    private readonly IProductRepository _products;
    private readonly ICategoryRepository _categories;
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IProductRepository products, ICategoryRepository categories, IUnitOfWork unitOfWork)
    {
        _products = products;
        _categories = categories;
        _unitOfWork = unitOfWork;
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        if (!await _categories.ExistsAsync(request.CategoryId, cancellationToken))
            throw new NotFoundException("Category", request.CategoryId);

        var normalizedSku = (request.Sku ?? string.Empty).Trim().ToUpperInvariant();
        if (await _products.GetBySkuAsync(normalizedSku, cancellationToken) is not null)
            throw new BusinessRuleViolationException("product.sku.duplicate", $"SKU '{normalizedSku}' already exists.");

        var product = Product.Create(request.CategoryId, normalizedSku, request.Name, request.Price, request.StockQuantity);

        _products.Add(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ProductResponse.FromEntity(product);
    }

    public async Task<ProductResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _products.GetByIdAsync(id, cancellationToken)
                      ?? throw new NotFoundException("Product", id);

        return ProductResponse.FromEntity(product);
    }

    public async Task<PagedResult<ProductResponse>> ListAsync(
        string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > MaxPageSize) pageSize = MaxPageSize;

        var items = await _products.ListAsync(search, page, pageSize, cancellationToken);
        var total = await _products.CountAsync(search, cancellationToken);

        return new PagedResult<ProductResponse>(
            items.Select(ProductResponse.FromEntity).ToList(),
            page,
            pageSize,
            total);
    }

    public async Task<ProductResponse> ChangePriceAsync(Guid id, ChangePriceRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _products.GetByIdAsync(id, cancellationToken)
                      ?? throw new NotFoundException("Product", id);

        product.ChangePrice(request.NewPrice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ProductResponse.FromEntity(product);
    }

    public async Task<ProductResponse> ReserveStockAsync(Guid id, ReserveStockRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _products.GetByIdAsync(id, cancellationToken)
                      ?? throw new NotFoundException("Product", id);

        product.Reserve(request.Quantity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ProductResponse.FromEntity(product);
    }
}
