namespace LocMp.Notification.Infrastructure.Cache;

internal sealed record CachedPreference(
    bool OrderUpdates,
    bool ReviewReplies,
    bool SystemAlerts,
    string? Email,
    bool EmailEnabled,
    bool EmailOrderUpdates,
    bool EmailReviewReplies)
{
    public static readonly CachedPreference Default = new(true, true, true, null, true, true, true);

    public bool CanEmailOrder => Email is not null && EmailEnabled && EmailOrderUpdates;
    public bool CanEmailReview => Email is not null && EmailEnabled && EmailReviewReplies;
    public bool CanEmailSystem => Email is not null && EmailEnabled;
    public bool CanEmailMandatory => Email is not null;
}
