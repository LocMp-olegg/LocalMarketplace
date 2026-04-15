using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Review.Application.DTOs;
using LocMp.Review.Domain.Entities;
using LocMp.Review.Domain.Enums;
using LocMp.Review.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LocMp.Review.Application.Reviews.Commands.UploadReviewPhotos;

public sealed class UploadReviewPhotosCommandHandler(
    ReviewDbContext db,
    IStorageService storage,
    IImageProcessor imageProcessor,
    IMapper mapper)
    : IRequestHandler<UploadReviewPhotosCommand, IReadOnlyList<ReviewPhotoDto>>
{
    private const int MaxPhotosPerReview = 5;
    private const int MaxImageDimension = 1200;

    public async Task<IReadOnlyList<ReviewPhotoDto>> Handle(
        UploadReviewPhotosCommand request, CancellationToken ct)
    {
        var review = await db.Reviews
                         .Include(r => r.Photos)
                         .FirstOrDefaultAsync(r => r.Id == request.ReviewId, ct)
                     ?? throw new NotFoundException($"Review '{request.ReviewId}' not found.");

        if (review.SubjectType != ReviewSubjectType.Product)
            throw new ConflictException("Photos can only be attached to product reviews.");

        if (!request.IsAdmin && review.ReviewerId != request.UploadedById)
            throw new ForbiddenException("You are not the author of this review.");

        var existingCount = review.Photos.Count;
        if (existingCount + request.Images.Count > MaxPhotosPerReview)
            throw new ConflictException(
                $"A review can have at most {MaxPhotosPerReview} photos. " +
                $"Currently {existingCount}, trying to add {request.Images.Count}.");

        var nextSortOrder = existingCount;
        var addedPhotos = new List<ReviewPhoto>();

        foreach (var image in request.Images)
        {
            await using var stream = image.OpenReadStream();
            var processed = await imageProcessor.ProcessAsync(stream, MaxImageDimension, MaxImageDimension, ct);

            var photoId = Guid.NewGuid();
            var objectKey = $"reviews/{request.ReviewId}/{photoId}.webp";

            using var ms = new MemoryStream(processed.Data);
            var storageUrl = await storage.UploadAsync(ms, objectKey, ProcessedImage.MimeType, ct);

            var photo = new ReviewPhoto(photoId)
            {
                ReviewId = request.ReviewId,
                StorageUrl = storageUrl,
                ObjectKey = objectKey,
                MimeType = ProcessedImage.MimeType,
                FileSize = processed.FileSize,
                SortOrder = nextSortOrder++,
                UploadedAt = DateTimeOffset.UtcNow
            };

            db.ReviewPhotos.Add(photo);
            addedPhotos.Add(photo);
        }

        await db.SaveChangesAsync(ct);

        return mapper.Map<List<ReviewPhotoDto>>(addedPhotos);
    }
}