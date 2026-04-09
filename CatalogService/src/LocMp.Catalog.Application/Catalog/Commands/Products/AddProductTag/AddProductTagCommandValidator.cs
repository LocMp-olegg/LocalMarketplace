using FluentValidation;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.AddProductTag;

public sealed class AddProductTagCommandValidator : AbstractValidator<AddProductTagCommand>
{
    public AddProductTagCommandValidator()
    {
        RuleFor(x => x.TagName).NotEmpty().MaximumLength(50);
    }
}
