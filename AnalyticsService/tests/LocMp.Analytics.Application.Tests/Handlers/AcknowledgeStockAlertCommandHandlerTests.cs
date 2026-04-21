using LocMp.Analytics.Application.Analytics.Commands;
using LocMp.Analytics.Domain.Entities;
using LocMp.Analytics.Infrastructure.Persistence;
using LocMp.BuildingBlocks.Application.Exceptions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LocMp.Analytics.Application.Tests.Handlers;

public sealed class AcknowledgeStockAlertCommandHandlerTests : IDisposable
{
    private readonly AnalyticsDbContext _db;
    private readonly AcknowledgeStockAlertCommandHandler _handler;

    public AcknowledgeStockAlertCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<AnalyticsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new AnalyticsDbContext(options);
        _handler = new AcknowledgeStockAlertCommandHandler(_db);
    }

    public void Dispose() => _db.Dispose();

    private StockAlert CreateAlert(Guid? sellerId = null, bool isAcknowledged = false)
    {
        var alert = new StockAlert(Guid.NewGuid())
        {
            SellerId = sellerId ?? Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = "Тестовый товар",
            IsAcknowledged = isAcknowledged,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _db.StockAlerts.Add(alert);
        _db.SaveChanges();
        return alert;
    }

    [Fact]
    public async Task Handle_UnacknowledgedAlert_SetsAcknowledgedAndTimestamp()
    {
        var alert = CreateAlert();
        var cmd = new AcknowledgeStockAlertCommand(alert.Id, alert.SellerId);
        var before = DateTimeOffset.UtcNow;

        await _handler.Handle(cmd, CancellationToken.None);

        var updated = await _db.StockAlerts.FindAsync(alert.Id);
        Assert.True(updated!.IsAcknowledged);
        Assert.NotNull(updated.AcknowledgedAt);
        Assert.True(updated.AcknowledgedAt >= before);
    }

    [Fact]
    public async Task Handle_AlertNotFound_ThrowsNotFoundException()
    {
        var cmd = new AcknowledgeStockAlertCommand(Guid.NewGuid(), Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WrongSeller_ThrowsNotFoundException()
    {
        var alert = CreateAlert();
        var cmd = new AcknowledgeStockAlertCommand(alert.Id, Guid.NewGuid()); // different seller

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AlreadyAcknowledged_IsIdempotent()
    {
        var alert = CreateAlert(isAcknowledged: true);
        var cmd = new AcknowledgeStockAlertCommand(alert.Id, alert.SellerId);

        // Should not throw
        await _handler.Handle(cmd, CancellationToken.None);

        var updated = await _db.StockAlerts.FindAsync(alert.Id);
        Assert.True(updated!.IsAcknowledged);
        Assert.Null(updated.AcknowledgedAt); // was already ack'd, timestamp not overwritten
    }
}
