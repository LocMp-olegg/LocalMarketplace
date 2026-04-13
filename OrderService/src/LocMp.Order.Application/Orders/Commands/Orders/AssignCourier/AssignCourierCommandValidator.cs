using FluentValidation;

namespace LocMp.Order.Application.Orders.Commands.Orders.AssignCourier;

public sealed class AssignCourierCommandValidator : AbstractValidator<AssignCourierCommand>
{
    public AssignCourierCommandValidator()
    {
        RuleFor(x => x.CourierId).NotEmpty();
        RuleFor(x => x.CourierName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CourierPhone).NotEmpty().MaximumLength(20);
    }
}
