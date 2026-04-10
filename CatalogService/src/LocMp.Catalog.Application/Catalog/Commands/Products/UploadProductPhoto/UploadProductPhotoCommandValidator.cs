using FluentValidation;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.UploadProductPhoto;

public sealed class UploadProductPhotoCommandValidator : AbstractValidator<UploadProductPhotoCommand>
{
    private static readonly string[] AllowedMimeTypes = ["image/jpeg", "image/png", "image/webp", "image/gif"];
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    public UploadProductPhotoCommandValidator()
    {
        RuleFor(x => x.Photos).NotEmpty().WithMessage("At least one photo is required.");
        RuleForEach(x => x.Photos).ChildRules(photo =>
        {
            photo.RuleFor(f => f.Length)
                .LessThanOrEqualTo(MaxFileSizeBytes)
                .WithMessage("Each photo must not exceed 10 MB.");
            photo.RuleFor(f => f.ContentType)
                .Must(ct => AllowedMimeTypes.Contains(ct))
                .WithMessage("Only JPEG, PNG, WebP, and GIF images are allowed.");
        });
    }
}
