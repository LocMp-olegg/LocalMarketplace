using FluentValidation;

namespace LocMp.Identity.Application.Identity.Commands.Shop.CreateShop;

public sealed class CreateShopCommandValidator : AbstractValidator<CreateShopCommand>
{
    public CreateShopCommandValidator()
    {
        RuleFor(x => x.BusinessName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PhoneNumber).NotEmpty().Matches(@"^\+?\d{10,15}$");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description is not null);
        RuleFor(x => x.Inn)
            .Length(10, 12).When(x => x.Inn is not null)
            .WithMessage("ИНН должен содержать 10 (юрлицо) или 12 (физлицо) цифр.")
            .Matches(@"^\d+$").When(x => x.Inn is not null);
        RuleFor(x => x.WorkingHours).MaximumLength(200).When(x => x.WorkingHours is not null);
        RuleFor(x => x.ServiceRadiusMeters).GreaterThan(0).When(x => x.ServiceRadiusMeters.HasValue);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue);
    }
}
