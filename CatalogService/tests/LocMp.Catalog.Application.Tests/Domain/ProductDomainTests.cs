using LocMp.Catalog.Domain.Entities;
using Xunit;

namespace LocMp.Catalog.Application.Tests.Domain;

public sealed class ProductDomainTests
{
    private static Product MakeProduct(int stock = 10) => new(Guid.NewGuid())
    {
        Name = "Test Product",
        SellerId = Guid.NewGuid(),
        StockQuantity = stock,
        Price = 50m,
        Unit = "шт",
        IsActive = true
    };

    // ── Reserve ──────────────────────────────────────────────────────────────

    [Fact]
    public void Reserve_SufficientStock_DecrementsAndReturnsNewStock()
    {
        var product = MakeProduct(10);

        var newStock = product.Reserve(3);

        Assert.Equal(7, newStock);
        Assert.Equal(7, product.StockQuantity);
    }

    [Fact]
    public void Reserve_ExactStock_ReturnsZero()
    {
        var product = MakeProduct(5);

        var newStock = product.Reserve(5);

        Assert.Equal(0, newStock);
    }

    [Fact]
    public void Reserve_InsufficientStock_ThrowsInvalidOperationException()
    {
        var product = MakeProduct(3);

        Assert.Throws<InvalidOperationException>(() => product.Reserve(5));
    }

    [Fact]
    public void Reserve_UpdatesUpdatedAt()
    {
        var product = MakeProduct(10);
        var before = DateTimeOffset.UtcNow;

        product.Reserve(1);

        Assert.NotNull(product.UpdatedAt);
        Assert.True(product.UpdatedAt >= before);
    }

    // ── Release ──────────────────────────────────────────────────────────────

    [Fact]
    public void Release_IncreasesStock()
    {
        var product = MakeProduct(5);

        var newStock = product.Release(3);

        Assert.Equal(8, newStock);
        Assert.Equal(8, product.StockQuantity);
    }

    [Fact]
    public void Release_FromZero_ReturnsReleasedQuantity()
    {
        var product = MakeProduct(0);

        var newStock = product.Release(4);

        Assert.Equal(4, newStock);
    }

    // ── AdjustStock ───────────────────────────────────────────────────────────

    [Fact]
    public void AdjustStock_PositiveDelta_IncreasesStock()
    {
        var product = MakeProduct(10);

        var newStock = product.AdjustStock(5);

        Assert.Equal(15, newStock);
    }

    [Fact]
    public void AdjustStock_NegativeDelta_DecreasesStock()
    {
        var product = MakeProduct(10);

        var newStock = product.AdjustStock(-4);

        Assert.Equal(6, newStock);
    }

    [Fact]
    public void AdjustStock_NegativeDeltaExceedingStock_ThrowsInvalidOperationException()
    {
        var product = MakeProduct(3);

        Assert.Throws<InvalidOperationException>(() => product.AdjustStock(-5));
    }

    [Fact]
    public void AdjustStock_ZeroDelta_NoChange()
    {
        var product = MakeProduct(7);

        var newStock = product.AdjustStock(0);

        Assert.Equal(7, newStock);
    }
}
