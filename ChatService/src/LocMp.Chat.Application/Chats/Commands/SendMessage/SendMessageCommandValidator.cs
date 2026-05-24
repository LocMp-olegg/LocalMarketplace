using FluentValidation;
using LocMp.Chat.Application.Constants;

namespace LocMp.Chat.Application.Chats.Commands.SendMessage;

public sealed class SendMessageCommandValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageCommandValidator()
    {
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Body) || (x.Attachments?.Count > 0))
            .WithMessage("A message must contain text or at least one attachment.");

        RuleFor(x => x.Body)
            .MaximumLength(AttachmentConstraints.MaxMessageBodyLength)
            .When(x => x.Body is not null);

        RuleFor(x => x.Attachments)
            .Must(files => files is null || files.Count <= AttachmentConstraints.MaxAttachmentsPerMessage)
            .WithMessage($"Maximum {AttachmentConstraints.MaxAttachmentsPerMessage} attachments per message.");

        RuleForEach(x => x.Attachments)
            .ChildRules(file =>
            {
                file.RuleFor(f => f.ContentType)
                    .Must(mime => AttachmentConstraints.AllAllowedMimeTypes.Contains(mime))
                    .WithMessage("Unsupported file type. Allowed: JPEG, PNG, GIF, WebP, MP4, WebM, MOV.");

                file.RuleFor(f => f.Length)
                    .Must((f, size) =>
                    {
                        if (AttachmentConstraints.AllowedImageMimeTypes.Contains(f.ContentType))
                            return size <= AttachmentConstraints.MaxImageSizeBytes;
                        if (AttachmentConstraints.AllowedVideoMimeTypes.Contains(f.ContentType))
                            return size <= AttachmentConstraints.MaxVideoSizeBytes;
                        return false;
                    })
                    .WithMessage("File exceeds the allowed size limit (images: 10 MB, videos: 100 MB).");
            })
            .When(x => x.Attachments is not null);
    }
}