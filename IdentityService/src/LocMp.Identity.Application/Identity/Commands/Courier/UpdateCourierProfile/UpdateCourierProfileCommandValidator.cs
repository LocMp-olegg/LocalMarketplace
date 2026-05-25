using FluentValidation;

namespace LocMp.Identity.Application.Identity.Commands.Courier.UpdateCourierProfile;

public sealed class UpdateCourierProfileCommandValidator : AbstractValidator<UpdateCourierProfileCommand>
{
    public UpdateCourierProfileCommandValidator()
    {
        RuleFor(x => x.ServiceRadiusMeters)
            .GreaterThanOrEqualTo(500).WithMessage("Service radius must be at least 500 meters.")
            .LessThanOrEqualTo(10_000).WithMessage("Service radius must not exceed 10 km.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue);

        RuleFor(x => x)
            .Must(x => x.Latitude.HasValue == x.Longitude.HasValue)
            .WithMessage("Latitude and Longitude must be provided together.")
            .WithName("Location");
    }
}