using FluentValidation;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.ReleaseStock;

public sealed class ReleaseStockCommandValidator : AbstractValidator<ReleaseStockCommand>
{
    public ReleaseStockCommandValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.OrderId).NotEmpty();
    }
}
