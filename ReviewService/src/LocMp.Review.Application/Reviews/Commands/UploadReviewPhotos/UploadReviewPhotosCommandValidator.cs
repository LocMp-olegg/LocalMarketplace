using FluentValidation;

namespace LocMp.Review.Application.Reviews.Commands.UploadReviewPhotos;

public sealed class UploadReviewPhotosCommandValidator : AbstractValidator<UploadReviewPhotosCommand>
{
    private static readonly string[] AllowedMimeTypes = ["image/jpeg", "image/png", "image/webp", "image/gif"];
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    public UploadReviewPhotosCommandValidator()
    {
        RuleFor(x => x.Images)
            .NotEmpty().WithMessage("At least one photo is required.")
            .Must(imgs => imgs.Count <= 5).WithMessage("Maximum 5 photos request.");

        RuleForEach(x => x.Images).ChildRules(photo =>
        {
            photo.RuleFor(f => f.Length)
                .LessThanOrEqualTo(MaxFileSizeBytes)
                .WithMessage("File size must not exceed 10 MB.");
            photo.RuleFor(f => f.ContentType)
                .Must(ct => AllowedMimeTypes.Contains(ct))
                .WithMessage("Only JPEG, PNG, WebP and GIF images are allowed.");
        });
    }
}