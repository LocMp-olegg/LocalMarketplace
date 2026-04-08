using FluentValidation;

namespace LocMp.Identity.Application.Identity.Commands.Users.UpdateUser;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.UserName)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(256);

        RuleFor(x => x.Email)
            .NotEmpty()
            .Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")
            .MaximumLength(256);

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\d{10}$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.UtcNow)
            .When(x => x.DateOfBirth.HasValue)
            .WithMessage("Date of birth must be in the past");
    }
}