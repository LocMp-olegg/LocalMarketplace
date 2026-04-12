using FluentValidation;

namespace LocMp.Order.Application.Orders.Commands.Orders.OpenDispute;

public sealed class OpenDisputeCommandValidator : AbstractValidator<OpenDisputeCommand>
{
    public OpenDisputeCommandValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(2000);
    }
}
