using FluentValidation;
using LocMp.Chat.Application.Constants;
using LocMp.Chat.Domain.Enums;

namespace LocMp.Chat.Application.Chats.Commands.CreateChat;

public sealed class CreateChatCommandValidator : AbstractValidator<CreateChatCommand>
{
    public CreateChatCommandValidator()
    {
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.InitiatorId).NotEmpty();
        RuleFor(x => x.InitiatorName).NotEmpty().MaximumLength(200);

        RuleFor(x => x.TargetUserId)
            .NotEmpty()
            .When(x => x.Type is ChatType.Direct or ChatType.Shop);

        RuleFor(x => x.ReferenceId)
            .NotEmpty()
            .When(x => x.Type == ChatType.Order);

        RuleFor(x => x.InitialMessage)
            .NotEmpty()
            .MaximumLength(AttachmentConstraints.MaxMessageBodyLength);
    }
}