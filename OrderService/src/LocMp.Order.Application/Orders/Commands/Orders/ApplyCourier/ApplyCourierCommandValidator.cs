using FluentValidation;

namespace LocMp.Order.Application.Orders.Commands.Orders.ApplyCourier;

public sealed class ApplyCourierCommandValidator : AbstractValidator<ApplyCourierCommand>
{
    public ApplyCourierCommandValidator()
    {
        RuleFor(x => x.CourierId).NotEmpty();
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.CourierName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CourierPhone).NotEmpty().MaximumLength(20);

        When(x => x.Latitude.HasValue || x.Longitude.HasValue, () =>
        {
            RuleFor(x => x.Latitude).NotNull()
                .InclusiveBetween(-90, 90);
            RuleFor(x => x.Longitude).NotNull()
                .InclusiveBetween(-180, 180);
        });
    }
}