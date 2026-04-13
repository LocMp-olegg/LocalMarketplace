using FluentValidation;
using LocMp.Order.Domain.Enums;

namespace LocMp.Order.Application.Orders.Commands.Orders.Checkout;

public sealed class CheckoutCommandValidator : AbstractValidator<CheckoutCommand>
{
    public CheckoutCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        When(x => x.DeliveryType == DeliveryType.NeighborCourier, () =>
        {
            RuleFor(x => x.DeliveryAddress).NotNull()
                .WithMessage("Delivery address is required for courier delivery.");

            RuleFor(x => x.DeliveryAddress!.City).NotEmpty().MaximumLength(100);
            RuleFor(x => x.DeliveryAddress!.Street).NotEmpty().MaximumLength(200);
            RuleFor(x => x.DeliveryAddress!.HouseNumber).NotEmpty().MaximumLength(20);
            RuleFor(x => x.DeliveryAddress!.RecipientName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.DeliveryAddress!.RecipientPhone).NotEmpty().MaximumLength(20);
        });
    }
}
