using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Catalog.Application.Catalog.Commands.Products.ReserveStock;
using LocMp.Catalog.Domain.Entities;
using LocMp.Catalog.Infrastructure.Persistence;
using LocMp.Contracts.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using Xunit;

namespace LocMp.Catalog.Application.Tests.Handlers;

public sealed class ReserveStockCommandHandlerTests : IDisposable
{
    private readonly CatalogDbContext _db;
    private readonly IEventBus _eventBus;
    private readonly IDistributedCache _cache;
    private readonly ReserveStockCommandHandler _handler;

    public ReserveStockCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new TestCatalogDbContext(options);
        _eventBus = Substitute.For<IEventBus>();
        _cache = Substitute.For<IDistributedCache>();
        _handler = new ReserveStockCommandHandler(_db, _eventBus, _cache);
    }

    public void Dispose() => _db.Dispose();

    private Product CreateProduct(int stock = 10, bool isActive = true, bool isDeleted = false,
        bool isMadeToOrder = false)
    {
        var product = new Product(Guid.NewGuid())
        {
            Name = "Test Product",
            SellerId = Guid.NewGuid(),
            ShopId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            StockQuantity = stock,
            Price = 100m,
            Unit = "шт",
            IsActive = isActive,
            IsDeleted = isDeleted,
            IsMadeToOrder = isMadeToOrder
        };
        _db.Products.Add(product);
        _db.SaveChanges();
        return product;
    }

    [Fact]
    public async Task Handle_SufficientStock_ReservesAndPublishesReservedEvent()
    {
        var product = CreateProduct(stock: 5);
        var cmd = new ReserveStockCommand(product.Id, 3, Guid.NewGuid());

        await _handler.Handle(cmd, CancellationToken.None);

        var updated = await _db.Products.FindAsync(product.Id);
        Assert.Equal(2, updated!.StockQuantity);

        await _eventBus.Received(1).PublishAsync(
            Arg.Is<StockReservedEvent>(e => e.ProductId == product.Id && e.ReservedQuantity == 3),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_StockReachesZero_PublishesDepletedEvent()
    {
        var product = CreateProduct(stock: 3);
        var cmd = new ReserveStockCommand(product.Id, 3, Guid.NewGuid());

        await _handler.Handle(cmd, CancellationToken.None);

        await _eventBus.Received(1).PublishAsync(
            Arg.Is<StockDepletedEvent>(e => e.ProductId == product.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_StockNotDepleted_DoesNotPublishDepletedEvent()
    {
        var product = CreateProduct(stock: 10);
        var cmd = new ReserveStockCommand(product.Id, 3, Guid.NewGuid());

        await _handler.Handle(cmd, CancellationToken.None);

        await _eventBus.DidNotReceive().PublishAsync(
            Arg.Any<StockDepletedEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ProductNotFound_ThrowsNotFoundException()
    {
        var cmd = new ReserveStockCommand(Guid.NewGuid(), 1, Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ProductNotActive_PublishesFailedEventAndThrows()
    {
        var product = CreateProduct(isActive: false);
        var cmd = new ReserveStockCommand(product.Id, 1, Guid.NewGuid());

        await Assert.ThrowsAsync<ConflictException>(() =>
            _handler.Handle(cmd, CancellationToken.None));

        await _eventBus.Received(1).PublishAsync(
            Arg.Is<StockReservationFailedEvent>(e => e.ProductId == product.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ProductDeleted_PublishesFailedEventAndThrows()
    {
        var product = CreateProduct(isDeleted: true);
        var cmd = new ReserveStockCommand(product.Id, 1, Guid.NewGuid());

        await Assert.ThrowsAsync<ConflictException>(() =>
            _handler.Handle(cmd, CancellationToken.None));

        await _eventBus.Received(1).PublishAsync(
            Arg.Is<StockReservationFailedEvent>(e => e.ProductId == product.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InsufficientStock_PublishesFailedEventAndThrows()
    {
        var product = CreateProduct(stock: 2);
        var cmd = new ReserveStockCommand(product.Id, 5, Guid.NewGuid());

        await Assert.ThrowsAsync<ConflictException>(() =>
            _handler.Handle(cmd, CancellationToken.None));

        await _eventBus.Received(1).PublishAsync(
            Arg.Is<StockReservationFailedEvent>(e => e.ProductId == product.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MadeToOrder_PublishesReservedEventWithoutDecrementingStock()
    {
        var product = CreateProduct(stock: 0, isMadeToOrder: true);
        var cmd = new ReserveStockCommand(product.Id, 2, Guid.NewGuid());

        await _handler.Handle(cmd, CancellationToken.None);

        var updated = await _db.Products.FindAsync(product.Id);
        Assert.Equal(0, updated!.StockQuantity); // stock unchanged

        await _eventBus.Received(1).PublishAsync(
            Arg.Is<StockReservedEvent>(e => e.ProductId == product.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SufficientStock_InvalidatesCacheEntry()
    {
        var product = CreateProduct(stock: 5);
        var cmd = new ReserveStockCommand(product.Id, 1, Guid.NewGuid());

        await _handler.Handle(cmd, CancellationToken.None);

        await _cache.Received(1).RemoveAsync(
            $"product:{product.Id}",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SufficientStock_CreatesStockHistoryEntry()
    {
        var product = CreateProduct(stock: 5);
        var orderId = Guid.NewGuid();
        var cmd = new ReserveStockCommand(product.Id, 2, orderId);

        await _handler.Handle(cmd, CancellationToken.None);

        var history = _db.StockHistory.Single(h => h.ProductId == product.Id);
        Assert.Equal(-2, history.QuantityDelta);
        Assert.Equal(3, history.QuantityAfter);
        Assert.Equal(orderId, history.ReferenceId);
    }
}
