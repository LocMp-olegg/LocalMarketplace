using FluentValidation;

namespace LocMp.Review.Application.Reviews.Commands.UpdateReviewResponse;

public sealed class UpdateReviewResponseCommandValidator : AbstractValidator<UpdateReviewResponseCommand>
{
    public UpdateReviewResponseCommandValidator()
    {
        RuleFor(x => x.Comment).NotEmpty().MaximumLength(2000);
    }
}
