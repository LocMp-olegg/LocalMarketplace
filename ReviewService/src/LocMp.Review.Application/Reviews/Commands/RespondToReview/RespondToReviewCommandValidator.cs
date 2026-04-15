using FluentValidation;

namespace LocMp.Review.Application.Reviews.Commands.RespondToReview;

public sealed class RespondToReviewCommandValidator : AbstractValidator<RespondToReviewCommand>
{
    public RespondToReviewCommandValidator()
    {
        RuleFor(x => x.Comment).NotEmpty().MaximumLength(2000);
    }
}