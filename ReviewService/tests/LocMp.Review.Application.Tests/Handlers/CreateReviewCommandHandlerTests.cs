using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Contracts.Review;
using LocMp.Review.Application.Reviews.Commands.CreateReview;
using LocMp.Review.Domain.Entities;
using LocMp.Review.Domain.Enums;
using LocMp.Review.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using Xunit;

namespace LocMp.Review.Application.Tests.Handlers;

public sealed class CreateReviewCommandHandlerTests : IDisposable
{
    private readonly ReviewDbContext _db;
    private readonly IEventBus _eventBus;
    private readonly IDistributedCache _cache;
    private readonly IMapper _mapper;
    private readonly CreateReviewCommandHandler _handler;

    public CreateReviewCommandHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ReviewDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ReviewDbContext(options);
        _eventBus = Substitute.For<IEventBus>();
        _cache = Substitute.For<IDistributedCache>();
        _mapper = Substitute.For<IMapper>();
        _handler = new CreateReviewCommandHandler(_db, _mapper, _eventBus, _cache);
    }

    public void Dispose() => _db.Dispose();

    private AllowedReview CreateAllowedReview(
        Guid? orderId = null, Guid? buyerId = null, Guid? sellerId = null,
        Guid? courierId = null, List<Guid>? productIds = null)
    {
        var allowed = new AllowedReview
        {
            OrderId = orderId ?? Guid.NewGuid(),
            BuyerId = buyerId ?? Guid.NewGuid(),
            SellerId = sellerId ?? Guid.NewGuid(),
            CourierId = courierId,
            ProductIds = productIds ?? [],
            AllowedAt = DateTimeOffset.UtcNow
        };
        _db.AllowedReviews.Add(allowed);
        _db.SaveChanges();
        return allowed;
    }

    [Fact]
    public async Task Handle_ValidSellerReview_CreatesReviewAndPublishesEvents()
    {
        var allowed = CreateAllowedReview();
        var cmd = new CreateReviewCommand(
            OrderId: allowed.OrderId,
            ReviewerId: allowed.BuyerId,
            ReviewerName: "Иван",
            SubjectType: ReviewSubjectType.Seller,
            SubjectId: allowed.SellerId,
            Rating: 5,
            Comment: "Отлично!");

        await _handler.Handle(cmd, CancellationToken.None);

        var review = await _db.Reviews.SingleAsync();
        Assert.Equal(allowed.OrderId, review.OrderId);
        Assert.Equal(5, review.Rating);
        Assert.Equal(ReviewSubjectType.Seller, review.SubjectType);

        await _eventBus.Received(1).PublishAsync(
            Arg.Is<RatingAggregateUpdatedEvent>(e => e.SubjectId == allowed.SellerId),
            Arg.Any<CancellationToken>());

        await _eventBus.Received(1).PublishAsync(
            Arg.Is<ReviewCreatedEvent>(e => e.ReviewId == review.Id && e.Rating == 5),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NoAllowedReview_ThrowsConflictException()
    {
        var cmd = new CreateReviewCommand(
            OrderId: Guid.NewGuid(),
            ReviewerId: Guid.NewGuid(),
            ReviewerName: "Иван",
            SubjectType: ReviewSubjectType.Seller,
            SubjectId: Guid.NewGuid(),
            Rating: 4,
            Comment: null);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WrongReviewer_ThrowsForbiddenException()
    {
        var allowed = CreateAllowedReview();
        var cmd = new CreateReviewCommand(
            OrderId: allowed.OrderId,
            ReviewerId: Guid.NewGuid(), // not the buyer
            ReviewerName: "Чужой",
            SubjectType: ReviewSubjectType.Seller,
            SubjectId: allowed.SellerId,
            Rating: 3,
            Comment: null);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WrongSubjectId_ThrowsForbiddenException()
    {
        var allowed = CreateAllowedReview();
        var cmd = new CreateReviewCommand(
            OrderId: allowed.OrderId,
            ReviewerId: allowed.BuyerId,
            ReviewerName: "Иван",
            SubjectType: ReviewSubjectType.Seller,
            SubjectId: Guid.NewGuid(), // not the seller from the order
            Rating: 4,
            Comment: null);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DuplicateReview_ThrowsConflictException()
    {
        var allowed = CreateAllowedReview();
        var cmd = new CreateReviewCommand(
            OrderId: allowed.OrderId,
            ReviewerId: allowed.BuyerId,
            ReviewerName: "Иван",
            SubjectType: ReviewSubjectType.Seller,
            SubjectId: allowed.SellerId,
            Rating: 5,
            Comment: null);

        await _handler.Handle(cmd, CancellationToken.None);

        // Second review for same subject
        var cmd2 = cmd with { OrderId = allowed.OrderId };
        await Assert.ThrowsAsync<ConflictException>(() =>
            _handler.Handle(cmd2, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ValidCourierReview_Succeeds()
    {
        var courierId = Guid.NewGuid();
        var allowed = CreateAllowedReview(courierId: courierId);
        var cmd = new CreateReviewCommand(
            OrderId: allowed.OrderId,
            ReviewerId: allowed.BuyerId,
            ReviewerName: "Иван",
            SubjectType: ReviewSubjectType.Courier,
            SubjectId: courierId,
            Rating: 4,
            Comment: null);

        await _handler.Handle(cmd, CancellationToken.None);

        Assert.Equal(1, await _db.Reviews.CountAsync());
    }

    [Fact]
    public async Task Handle_ValidProductReview_Succeeds()
    {
        var productId = Guid.NewGuid();
        var allowed = CreateAllowedReview(productIds: [productId]);
        var cmd = new CreateReviewCommand(
            OrderId: allowed.OrderId,
            ReviewerId: allowed.BuyerId,
            ReviewerName: "Иван",
            SubjectType: ReviewSubjectType.Product,
            SubjectId: productId,
            Rating: 5,
            Comment: "Хороший товар");

        await _handler.Handle(cmd, CancellationToken.None);

        Assert.Equal(1, await _db.Reviews.CountAsync());
    }

    [Fact]
    public async Task Handle_FirstReview_CreatesRatingAggregate()
    {
        var allowed = CreateAllowedReview();
        var cmd = new CreateReviewCommand(
            OrderId: allowed.OrderId,
            ReviewerId: allowed.BuyerId,
            ReviewerName: "Иван",
            SubjectType: ReviewSubjectType.Seller,
            SubjectId: allowed.SellerId,
            Rating: 4,
            Comment: null);

        await _handler.Handle(cmd, CancellationToken.None);

        var agg = await _db.RatingAggregates.SingleAsync();
        Assert.Equal(allowed.SellerId, agg.SubjectId);
        Assert.Equal(1, agg.ReviewCount);
        Assert.Equal(4m, agg.AverageRating);
    }

    [Fact]
    public async Task Handle_ValidReview_InvalidatesRatingCache()
    {
        var allowed = CreateAllowedReview();
        var cmd = new CreateReviewCommand(
            OrderId: allowed.OrderId,
            ReviewerId: allowed.BuyerId,
            ReviewerName: "Иван",
            SubjectType: ReviewSubjectType.Seller,
            SubjectId: allowed.SellerId,
            Rating: 5,
            Comment: null);

        await _handler.Handle(cmd, CancellationToken.None);

        await _cache.Received(1).RemoveAsync(
            $"rating:{ReviewSubjectType.Seller}:{allowed.SellerId}",
            Arg.Any<CancellationToken>());
    }
}
