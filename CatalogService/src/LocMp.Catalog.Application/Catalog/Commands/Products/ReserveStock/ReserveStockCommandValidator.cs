using FluentValidation;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.ReserveStock;

public sealed class ReserveStockCommandValidator : AbstractValidator<ReserveStockCommand>
{
    public ReserveStockCommandValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
