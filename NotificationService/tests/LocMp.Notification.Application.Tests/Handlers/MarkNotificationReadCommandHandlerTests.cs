using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.Notification.Application.Notifications.Commands.MarkNotificationRead;
using LocMp.Notification.Domain.Enums;
using LocMp.Notification.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using Xunit;
using NotificationEntity = LocMp.Notification.Domain.Entities.Notification;

namespace LocMp.Notification.Application.Tests.Handlers;

public sealed class MarkNotificationReadCommandHandlerTests : IDisposable
{
    private readonly TestNotificationDbContext _db;
    private readonly IDistributedCache _cache;
    private readonly MarkNotificationReadCommandHandler _handler;

    public MarkNotificationReadCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new TestNotificationDbContext(options);
        _cache = Substitute.For<IDistributedCache>();
        _handler = new MarkNotificationReadCommandHandler(_db, _cache);
    }

    public void Dispose() => _db.Dispose();

    private NotificationEntity CreateNotification(Guid userId, bool isRead = false)
    {
        var notification = new NotificationEntity(Guid.NewGuid())
        {
            UserId = userId,
            Type = NotificationType.OrderPlaced,
            Title = "Новый заказ",
            Body = "Поступил новый заказ",
            DeliveryChannel = DeliveryChannel.InApp,
            DeliveryStatus = DeliveryStatus.Sent,
            IsRead = isRead,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _db.Notifications.Add(notification);
        _db.SaveChanges();
        return notification;
    }

    [Fact]
    public async Task Handle_UnreadNotification_MarksAsReadAndInvalidatesCache()
    {
        var userId = Guid.NewGuid();
        var notification = CreateNotification(userId);
        var cmd = new MarkNotificationReadCommand(notification.Id, userId);

        await _handler.Handle(cmd, CancellationToken.None);

        var updated = await _db.Notifications.FindAsync(notification.Id);
        Assert.True(updated!.IsRead);
        Assert.NotNull(updated.ReadAt);

        await _cache.Received(1).RemoveAsync(
            $"notif:unread:{userId}",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NotificationNotFound_ThrowsNotFoundException()
    {
        var cmd = new MarkNotificationReadCommand(Guid.NewGuid(), Guid.NewGuid());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WrongUser_ThrowsForbiddenException()
    {
        var notification = CreateNotification(Guid.NewGuid());
        var cmd = new MarkNotificationReadCommand(notification.Id, Guid.NewGuid()); // different user

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AlreadyRead_IsIdempotentAndDoesNotInvalidateCache()
    {
        var userId = Guid.NewGuid();
        var notification = CreateNotification(userId, isRead: true);
        var cmd = new MarkNotificationReadCommand(notification.Id, userId);

        await _handler.Handle(cmd, CancellationToken.None);

        // Cache should NOT be invalidated for already-read notifications
        await _cache.DidNotReceive().RemoveAsync(
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnreadNotification_SetsReadAtTimestamp()
    {
        var userId = Guid.NewGuid();
        var notification = CreateNotification(userId);
        var before = DateTimeOffset.UtcNow;
        var cmd = new MarkNotificationReadCommand(notification.Id, userId);

        await _handler.Handle(cmd, CancellationToken.None);

        var updated = await _db.Notifications.FindAsync(notification.Id);
        Assert.NotNull(updated!.ReadAt);
        Assert.True(updated.ReadAt >= before);
    }
}
