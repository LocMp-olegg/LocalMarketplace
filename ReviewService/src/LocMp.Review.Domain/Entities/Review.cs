using LocMp.BuildingBlocks;
using LocMp.Review.Domain.Enums;

namespace LocMp.Review.Domain.Entities;

public class Review(Guid id) : AggregateRoot<Guid>(id)
{
    public Guid OrderId { get; set; }

    public Guid ReviewerId { get; set; }
    public string ReviewerName { get; set; } = null!;

    public ReviewSubjectType SubjectType { get; set; }
    public Guid SubjectId { get; set; }

    public int Rating { get; set; }
    public string? Comment { get; set; }

    public bool IsVisible { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public virtual ICollection<ReviewPhoto> Photos { get; set; } = [];
    public virtual ReviewResponse? Response { get; set; }

    /// <summary>Скрыть отзыв (модерация).</summary>
    public void Hide(DateTimeOffset now)
    {
        IsVisible = false;
        UpdatedAt = now;
    }

    /// <summary>Восстановить видимость отзыва (модерация).</summary>
    public void Restore(DateTimeOffset now)
    {
        IsVisible = true;
        UpdatedAt = now;
    }
}