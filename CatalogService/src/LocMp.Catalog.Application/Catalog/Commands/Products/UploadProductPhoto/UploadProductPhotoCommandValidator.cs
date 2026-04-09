using FluentValidation;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.UploadProductPhoto;

public sealed class UploadProductPhotoCommandValidator : AbstractValidator<UploadProductPhotoCommand>
{
    private static readonly string[] AllowedMimeTypes = ["image/jpeg", "image/png", "image/webp", "image/gif"];
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    public UploadProductPhotoCommandValidator()
    {
        RuleFor(x => x.Photo).NotNull().WithMessage("Photo is required.");
        RuleFor(x => x.Photo.Length)
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage("Photo must not exceed 10 MB.");
        RuleFor(x => x.Photo.ContentType)
            .Must(ct => AllowedMimeTypes.Contains(ct))
            .WithMessage("Only JPEG, PNG, WebP, and GIF images are allowed.");
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
