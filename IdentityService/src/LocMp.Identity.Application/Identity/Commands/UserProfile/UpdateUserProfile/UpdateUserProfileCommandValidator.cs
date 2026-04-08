using FluentValidation;

namespace LocMp.Identity.Application.Identity.Commands.UserProfile.UpdateUserProfile;

public sealed class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(256)
            .When(x => x.FirstName != null);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(256)
            .When(x => x.LastName != null);

        RuleFor(x => x.BirthDate)
            .LessThan(DateTime.UtcNow)
            .When(x => x.BirthDate.HasValue)
            .WithMessage("Date of birth must be in the past");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\d{10}$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}