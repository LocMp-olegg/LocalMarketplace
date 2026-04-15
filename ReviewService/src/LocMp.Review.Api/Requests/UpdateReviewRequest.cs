namespace LocMp.Review.Api.Requests;

public sealed record UpdateReviewRequest(int Rating, string? Comment);
