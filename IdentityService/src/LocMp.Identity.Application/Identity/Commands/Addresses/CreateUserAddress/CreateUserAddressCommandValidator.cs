using FluentValidation;

namespace LocMp.Identity.Application.Identity.Commands.Addresses.CreateUserAddress;

public sealed class CreateUserAddressCommandValidator : AbstractValidator<CreateUserAddressCommand>
{
    public CreateUserAddressCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(100);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Street).NotEmpty().MaximumLength(250);
        RuleFor(x => x.HouseNumber).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Apartment).MaximumLength(20).When(x => x.Apartment is not null);
        RuleFor(x => x.Entrance).MaximumLength(10).When(x => x.Entrance is not null);
        RuleFor(x => x.Floor).MaximumLength(10).When(x => x.Floor is not null);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue);
    }
}
