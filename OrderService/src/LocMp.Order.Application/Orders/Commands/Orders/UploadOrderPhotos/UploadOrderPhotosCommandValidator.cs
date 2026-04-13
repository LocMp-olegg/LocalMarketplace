using FluentValidation;

namespace LocMp.Order.Application.Orders.Commands.Orders.UploadOrderPhotos;

public sealed class UploadOrderPhotosCommandValidator : AbstractValidator<UploadOrderPhotosCommand>
{
    private static readonly string[] AllowedMimeTypes = ["image/jpeg", "image/png", "image/webp", "image/gif"];
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    public UploadOrderPhotosCommandValidator()
    {
        RuleFor(x => x.Images).NotEmpty().WithMessage("At least one photo is required.");
        RuleForEach(x => x.Images).ChildRules(photo =>
        {
            photo.RuleFor(f => f.Length).LessThanOrEqualTo(MaxFileSizeBytes)
                .WithMessage("File size must not exceed 10 MB.");
            photo.RuleFor(f => f.ContentType)
                .Must(ct => AllowedMimeTypes.Contains(ct))
                .WithMessage("Unsupported image format.");
        });
    }
}