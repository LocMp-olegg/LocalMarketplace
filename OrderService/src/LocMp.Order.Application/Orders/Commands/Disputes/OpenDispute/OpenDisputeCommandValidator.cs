using FluentValidation;
using LocMp.Contracts.Orders;

namespace LocMp.Order.Application.Orders.Commands.Disputes.OpenDispute;

public sealed class OpenDisputeCommandValidator : AbstractValidator<OpenDisputeCommand>
{
    public OpenDisputeCommandValidator()
    {
        RuleFor(x => x.DisputeType).IsInEnum()
            .WithMessage("Invalid dispute type.");
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(2000);
    }
}