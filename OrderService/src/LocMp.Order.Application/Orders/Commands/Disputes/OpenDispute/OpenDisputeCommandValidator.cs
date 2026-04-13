using FluentValidation;

namespace LocMp.Order.Application.Orders.Commands.Disputes.OpenDispute;

public sealed class OpenDisputeCommandValidator : AbstractValidator<OpenDisputeCommand>
{
    public OpenDisputeCommandValidator()
    {
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(2000);
    }
}