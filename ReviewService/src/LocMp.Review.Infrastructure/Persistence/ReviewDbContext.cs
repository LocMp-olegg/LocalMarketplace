using LocMp.Review.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using ReviewEntity = LocMp.Review.Domain.Entities.Review;

namespace LocMp.Review.Infrastructure.Persistence;

public class ReviewDbContext(DbContextOptions<ReviewDbContext> options) : DbContext(options)
{
    public DbSet<ReviewEntity> Reviews => Set<ReviewEntity>();
    public DbSet<ReviewPhoto> ReviewPhotos => Set<ReviewPhoto>();
    public DbSet<ReviewResponse> ReviewResponses => Set<ReviewResponse>();
    public DbSet<RatingAggregate> RatingAggregates => Set<RatingAggregate>();
    public DbSet<AllowedReview> AllowedReviews => Set<AllowedReview>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("reviews");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReviewDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}