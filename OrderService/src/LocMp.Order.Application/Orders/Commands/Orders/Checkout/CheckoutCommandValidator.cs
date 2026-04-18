using FluentValidation;
using LocMp.Order.Domain.Enums;

namespace LocMp.Order.Application.Orders.Commands.Orders.Checkout;

public sealed class CheckoutCommandValidator : AbstractValidator<CheckoutCommand>
{
    public CheckoutCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Groups).NotEmpty().WithMessage("At least one group must be specified for checkout.");

        RuleForEach(x => x.Groups).ChildRules(group =>
        {
            group.RuleFor(g => g.SellerId).NotEmpty();

            group.When(g => g.DeliveryType == DeliveryType.NeighborCourier, () =>
            {
                group.RuleFor(g => g.DeliveryAddress).NotNull()
                    .WithMessage("Delivery address is required for courier delivery.");
                group.RuleFor(g => g.DeliveryAddress!.City).NotEmpty().MaximumLength(100);
                group.RuleFor(g => g.DeliveryAddress!.Street).NotEmpty().MaximumLength(200);
                group.RuleFor(g => g.DeliveryAddress!.HouseNumber).NotEmpty().MaximumLength(20);
                group.RuleFor(g => g.DeliveryAddress!.RecipientName).NotEmpty().MaximumLength(200);
                group.RuleFor(g => g.DeliveryAddress!.RecipientPhone).NotEmpty().MaximumLength(20);
            });
        });
    }
}
