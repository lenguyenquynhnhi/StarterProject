using DevOpsLab.Domain.Common;

namespace DevOpsLab.Domain.Entities;

/// <summary>
/// Catalog product. All state transitions go through methods on this class so
/// that the business rules cannot be bypassed by callers — built-in quality
/// applied to the domain model.
/// </summary>
public sealed class Product
{
    public const int SkuMinLength = 3;
    public const int SkuMaxLength = 32;
    public const int NameMaxLength = 200;

    /// <summary>A single price change may not raise the price by more than this factor.</summary>
    public const decimal MaxPriceIncreaseFactor = 1.50m;

    private Product() { Sku = string.Empty; Name = string.Empty; }

    private Product(Guid id, Guid categoryId, string sku, string name, decimal price, int stockQuantity)
    {
        Id = id;
        CategoryId = categoryId;
        Sku = sku;
        Name = name;
        Price = price;
        StockQuantity = stockQuantity;
        IsActive = true;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Sku { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public static Product Create(Guid categoryId, string sku, string name, decimal price, int stockQuantity)
    {
        if (categoryId == Guid.Empty)
            throw new BusinessRuleViolationException("product.category.required", "A product must belong to a category.");

        sku = NormalizeSku(sku);

        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessRuleViolationException("product.name.required", "Product name is required.");

        name = name.Trim();

        if (name.Length > NameMaxLength)
            throw new BusinessRuleViolationException(
                "product.name.tooLong",
                $"Product name must be at most {NameMaxLength} characters.");

        if (price <= 0m)
            throw new BusinessRuleViolationException("product.price.positive", "Price must be greater than zero.");

        if (stockQuantity < 0)
            throw new BusinessRuleViolationException("product.stock.negative", "Stock quantity cannot be negative.");

        return new Product(Guid.NewGuid(), categoryId, sku, name, decimal.Round(price, 2), stockQuantity);
    }

    public void ChangePrice(decimal newPrice)
    {
        if (newPrice <= 0m)
            throw new BusinessRuleViolationException("product.price.positive", "Price must be greater than zero.");

        if (newPrice > Price * MaxPriceIncreaseFactor)
            throw new BusinessRuleViolationException(
                "product.price.increaseTooLarge",
                $"A single price change may not exceed {(MaxPriceIncreaseFactor - 1m) * 100m:0}% of the current price.");

        Price = decimal.Round(newPrice, 2);
    }

    public void Reserve(int quantity)
    {
        if (!IsActive)
            throw new BusinessRuleViolationException("product.inactive", "Cannot reserve stock for an inactive product.");

        if (quantity <= 0)
            throw new BusinessRuleViolationException("product.reserve.positive", "Reserved quantity must be greater than zero.");

        if (quantity > StockQuantity)
            throw new BusinessRuleViolationException(
                "product.reserve.insufficientStock",
                $"Only {StockQuantity} unit(s) available, {quantity} requested.");

        StockQuantity -= quantity;
    }

    public void Restock(int quantity)
    {
        if (quantity <= 0)
            throw new BusinessRuleViolationException("product.restock.positive", "Restock quantity must be greater than zero.");

        StockQuantity += quantity;
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    private static string NormalizeSku(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new BusinessRuleViolationException("product.sku.required", "SKU is required.");

        sku = sku.Trim().ToUpperInvariant();

        if (sku.Length is < SkuMinLength or > SkuMaxLength)
            throw new BusinessRuleViolationException(
                "product.sku.length",
                $"SKU must be between {SkuMinLength} and {SkuMaxLength} characters.");

        if (!sku.All(c => char.IsAsciiLetterOrDigit(c) || c == '-'))
            throw new BusinessRuleViolationException(
                "product.sku.format",
                "SKU may only contain letters, digits and hyphens.");

        return sku;
    }
}
