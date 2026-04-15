using LocMp.Review.Application.DTOs;
using LocMp.Review.Domain.Enums;
using MediatR;

namespace LocMp.Review.Application.Reviews.Commands.CreateReview;

public sealed record CreateReviewCommand(
    Guid OrderId,
    Guid ReviewerId,
    string ReviewerName,
    ReviewSubjectType SubjectType,
    Guid SubjectId,
    int Rating,
    string? Comment) : IRequest<ReviewDto>;