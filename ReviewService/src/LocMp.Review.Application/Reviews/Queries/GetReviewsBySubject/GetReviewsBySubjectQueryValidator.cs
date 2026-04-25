using FluentValidation;

namespace LocMp.Review.Application.Reviews.Queries.GetReviewsBySubject;

public sealed class GetReviewsBySubjectQueryValidator : AbstractValidator<GetReviewsBySubjectQuery>
{
    public GetReviewsBySubjectQueryValidator()
    {
        RuleFor(x => x.SubjectId).NotEmpty();
        RuleFor(x => x.RatingFilter)
            .InclusiveBetween(1, 5)
            .When(x => x.RatingFilter.HasValue)
            .WithMessage("Rating filter must be between 1 and 5.");
    }
}
