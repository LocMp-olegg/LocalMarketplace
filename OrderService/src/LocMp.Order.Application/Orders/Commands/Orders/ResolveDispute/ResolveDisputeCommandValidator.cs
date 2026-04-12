using FluentValidation;

namespace LocMp.Order.Application.Orders.Commands.Orders.ResolveDispute;

public sealed class ResolveDisputeCommandValidator : AbstractValidator<ResolveDisputeCommand>
{
    public ResolveDisputeCommandValidator()
    {
        RuleFor(x => x.Resolution).NotEmpty().MaximumLength(2000);
    }
}
