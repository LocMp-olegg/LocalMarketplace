using FluentValidation;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.UpdateStock;

public sealed class UpdateStockCommandValidator : AbstractValidator<UpdateStockCommand>
{
    public UpdateStockCommandValidator()
    {
        RuleFor(x => x.QuantityDelta).NotEqual(0).WithMessage("QuantityDelta must not be zero.");
    }
}
