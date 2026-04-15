using LocMp.Review.Domain.Enums;

namespace LocMp.Review.Domain.Entities;

public class RatingAggregate
{
    public Guid SubjectId { get; set; }
    public ReviewSubjectType SubjectType { get; set; }

    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }

    public int OneStar { get; set; }
    public int TwoStar { get; set; }
    public int ThreeStar { get; set; }
    public int FourStar { get; set; }
    public int FiveStar { get; set; }

    public DateTimeOffset LastCalculatedAt { get; set; } = DateTimeOffset.UtcNow;

    public void AddRating(int rating, DateTimeOffset now)
    {
        if (rating is < 1 or > 5)
            throw new InvalidOperationException($"Rating must be between 1 and 5, got {rating}.");

        switch (rating)
        {
            case 1: OneStar++; break;
            case 2: TwoStar++; break;
            case 3: ThreeStar++; break;
            case 4: FourStar++; break;
            case 5: FiveStar++; break;
        }

        ReviewCount++;
        AverageRating = (decimal)(OneStar + TwoStar * 2 + ThreeStar * 3 + FourStar * 4 + FiveStar * 5)
                        / ReviewCount;
        LastCalculatedAt = now;
    }

    public void RemoveRating(int rating, DateTimeOffset now)
    {
        if (rating is < 1 or > 5)
            throw new InvalidOperationException($"Rating must be between 1 and 5, got {rating}.");

        switch (rating)
        {
            case 1: OneStar = Math.Max(0, OneStar - 1); break;
            case 2: TwoStar = Math.Max(0, TwoStar - 1); break;
            case 3: ThreeStar = Math.Max(0, ThreeStar - 1); break;
            case 4: FourStar = Math.Max(0, FourStar - 1); break;
            case 5: FiveStar = Math.Max(0, FiveStar - 1); break;
        }

        ReviewCount = Math.Max(0, ReviewCount - 1);
        AverageRating = ReviewCount == 0
            ? 0
            : (decimal)(OneStar + TwoStar * 2 + ThreeStar * 3 + FourStar * 4 + FiveStar * 5)
              / ReviewCount;
        LastCalculatedAt = now;
    }
}