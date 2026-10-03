using DevOpsLab.Domain.Common;
using DevOpsLab.Domain.Entities;
using Xunit;

namespace DevOpsLab.UnitTests.Domain;

/// <summary>
/// Every test follows the 3A structure: Arrange - Act - Assert.
/// Session 3 extends this class; do not delete the failure-case tests.
/// </summary>
public class ProductTests
{
    private static readonly Guid CategoryId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_NormalisesSkuAndActivatesProduct()
    {
        // Arrange
        const string rawSku = " lap-14-pro ";

        // Act
        var product = Product.Create(CategoryId, rawSku, " 14-inch Pro Laptop ", 1899.004m, 25);

        // Assert
        Assert.Equal("LAP-14-PRO", product.Sku);
        Assert.Equal("14-inch Pro Laptop", product.Name);
        Assert.Equal(1899.00m, product.Price);
        Assert.Equal(25, product.StockQuantity);
        Assert.True(product.IsActive);
        Assert.NotEqual(Guid.Empty, product.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("AB")]
    [InlineData("THIS-SKU-IS-DEFINITELY-FAR-TOO-LONG-FOR-THE-RULE")]
    [InlineData("BAD SKU")]
    [InlineData("BAD_SKU")]
    public void Create_WithInvalidSku_ThrowsBusinessRuleViolation(string sku)
    {
        var exception = Assert.Throws<BusinessRuleViolationException>(
            () => Product.Create(CategoryId, sku, "Valid name", 10m, 1));

        Assert.StartsWith("product.sku", exception.Rule);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Create_WithNonPositivePrice_ThrowsBusinessRuleViolation(decimal price)
    {
        var exception = Assert.Throws<BusinessRuleViolationException>(
            () => Product.Create(CategoryId, "SKU-001", "Valid name", price, 1));

        Assert.Equal("product.price.positive", exception.Rule);
    }

    [Fact]
    public void Create_WithEmptyCategory_ThrowsBusinessRuleViolation()
    {
        var exception = Assert.Throws<BusinessRuleViolationException>(
            () => Product.Create(Guid.Empty, "SKU-001", "Valid name", 10m, 1));

        Assert.Equal("product.category.required", exception.Rule);
    }

    [Fact]
    public void Create_WithNegativeStock_ThrowsBusinessRuleViolation()
    {
        var exception = Assert.Throws<BusinessRuleViolationException>(
            () => Product.Create(CategoryId, "SKU-001", "Valid name", 10m, -1));

        Assert.Equal("product.stock.negative", exception.Rule);
    }

    [Fact]
    public void ChangePrice_WithinAllowedIncrease_UpdatesPrice()
    {
        var product = Product.Create(CategoryId, "SKU-001", "Valid name", 100m, 10);

        product.ChangePrice(150m);

        Assert.Equal(150m, product.Price);
    }

    [Fact]
    public void ChangePrice_AboveMaximumIncrease_ThrowsBusinessRuleViolation()
    {
        var product = Product.Create(CategoryId, "SKU-001", "Valid name", 100m, 10);

        var exception = Assert.Throws<BusinessRuleViolationException>(() => product.ChangePrice(150.01m));

        Assert.Equal("product.price.increaseTooLarge", exception.Rule);
        Assert.Equal(100m, product.Price);
    }

    [Fact]
    public void Reserve_WithSufficientStock_DecreasesStock()
    {
        var product = Product.Create(CategoryId, "SKU-001", "Valid name", 100m, 10);

        product.Reserve(4);

        Assert.Equal(6, product.StockQuantity);
    }

    [Fact]
    public void Reserve_MoreThanAvailable_ThrowsAndLeavesStockUnchanged()
    {
        var product = Product.Create(CategoryId, "SKU-001", "Valid name", 100m, 10);

        var exception = Assert.Throws<BusinessRuleViolationException>(() => product.Reserve(11));

        Assert.Equal("product.reserve.insufficientStock", exception.Rule);
        Assert.Equal(10, product.StockQuantity);
    }

    [Fact]
    public void Reserve_OnInactiveProduct_ThrowsBusinessRuleViolation()
    {
        var product = Product.Create(CategoryId, "SKU-001", "Valid name", 100m, 10);
        product.Deactivate();

        var exception = Assert.Throws<BusinessRuleViolationException>(() => product.Reserve(1));

        Assert.Equal("product.inactive", exception.Rule);
    }

    [Fact]
    public void Restock_WithPositiveQuantity_IncreasesStock()
    {
        var product = Product.Create(CategoryId, "SKU-001", "Valid name", 100m, 10);

        product.Restock(5);

        Assert.Equal(15, product.StockQuantity);
    }
}
