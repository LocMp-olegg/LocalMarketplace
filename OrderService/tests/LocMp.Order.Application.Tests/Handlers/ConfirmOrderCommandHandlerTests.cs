using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Orders;
using LocMp.Order.Application.Orders.Commands.Orders.ConfirmOrder;
using LocMp.Order.Domain.Enums;
using LocMp.Order.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;
using OrderEntity = LocMp.Order.Domain.Entities.Order;

namespace LocMp.Order.Application.Tests.Handlers;

public sealed class ConfirmOrderCommandHandlerTests : IDisposable
{
    private readonly OrderDbContext _db;
    private readonly IEventBus _eventBus;
    private readonly ConfirmOrderCommandHandler _handler;

    public ConfirmOrderCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new OrderDbContext(options);
        _eventBus = Substitute.For<IEventBus>();
        _handler = new ConfirmOrderCommandHandler(_db, _eventBus);
    }

    public void Dispose() => _db.Dispose();

    private OrderEntity CreatePendingOrder(Guid? sellerId = null)
    {
        var order = new OrderEntity(Guid.NewGuid())
        {
            BuyerId = Guid.NewGuid(),
            SellerId = sellerId ?? Guid.NewGuid(),
            Status = OrderStatus.Pending,
            TotalAmount = 100m,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _db.Orders.Add(order);
        _db.SaveChanges();
        return order;
    }

    [Fact]
    public async Task Handle_ValidCommand_ConfirmsOrderAndPublishesEvent()
    {
        var order = CreatePendingOrder();
        var cmd = new ConfirmOrderCommand(order.Id, order.SellerId);

        await _handler.Handle(cmd, CancellationToken.None);

        var updated = await _db.Orders.FindAsync(order.Id);
        Assert.Equal(OrderStatus.Confirmed, updated!.Status);

        await _eventBus.Received(1).PublishAsync(
            Arg.Is<OrderStatusChangedEvent>(e =>
                e.OrderId == order.Id &&
                e.ToStatus == nameof(OrderStatus.Confirmed) &&
                e.FromStatus == nameof(OrderStatus.Pending)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OrderNotFound_ThrowsNotFoundException()
    {
        var cmd = new ConfirmOrderCommand(Guid.NewGuid(), Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WrongSeller_ThrowsForbiddenException()
    {
        var order = CreatePendingOrder();
        var cmd = new ConfirmOrderCommand(order.Id, Guid.NewGuid()); // different seller

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_InvalidTransition_ThrowsConflictException()
    {
        var order = CreatePendingOrder();
        // pre-confirm the order so next transition from Confirmed→Confirmed is invalid
        order.TransitionTo(OrderStatus.Confirmed, order.SellerId, DateTimeOffset.UtcNow);
        await _db.SaveChangesAsync();

        var cmd = new ConfirmOrderCommand(order.Id, order.SellerId);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsStatusHistoryEntry()
    {
        var order = CreatePendingOrder();
        var cmd = new ConfirmOrderCommand(order.Id, order.SellerId);

        await _handler.Handle(cmd, CancellationToken.None);

        var history = await _db.OrderStatusHistory
            .Where(h => h.OrderId == order.Id)
            .ToListAsync();

        Assert.Single(history);
        Assert.Equal(OrderStatus.Confirmed, history[0].ToStatus);
        Assert.Equal(OrderStatus.Pending, history[0].FromStatus);
    }

    [Fact]
    public async Task Handle_WrongSeller_DoesNotPublishEvent()
    {
        var order = CreatePendingOrder();
        var cmd = new ConfirmOrderCommand(order.Id, Guid.NewGuid());

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _handler.Handle(cmd, CancellationToken.None));

        await _eventBus.DidNotReceive().PublishAsync(
            Arg.Any<OrderStatusChangedEvent>(),
            Arg.Any<CancellationToken>());
    }
}
