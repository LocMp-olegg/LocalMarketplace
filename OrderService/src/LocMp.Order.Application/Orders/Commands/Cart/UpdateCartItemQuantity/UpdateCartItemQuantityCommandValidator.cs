using FluentValidation;

namespace LocMp.Order.Application.Orders.Commands.Cart.UpdateCartItemQuantity;

public sealed class UpdateCartItemQuantityCommandValidator : AbstractValidator<UpdateCartItemQuantityCommand>
{
    public UpdateCartItemQuantityCommandValidator()
    {
        RuleFor(x => x.Quantity).InclusiveBetween(1, 100);
    }
}
