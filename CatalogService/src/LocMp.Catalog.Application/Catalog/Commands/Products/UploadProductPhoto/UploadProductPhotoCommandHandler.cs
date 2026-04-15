using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Catalog.Domain.Entities;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using SixLabors.ImageSharp;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.UploadProductPhoto;

public sealed class UploadProductPhotoCommandHandler(
    CatalogDbContext db,
    IStorageService storageService,
    IImageProcessor imageProcessor,
    IDistributedCache cache)
    : IRequestHandler<UploadProductPhotoCommand, IReadOnlyList<Guid>>
{
    private const int MaxWidth = 1200;
    private const int MaxHeight = 1200;
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;
    private const int MaxPhotosPerProduct = 10;

    public async Task<IReadOnlyList<Guid>> Handle(UploadProductPhotoCommand request, CancellationToken ct)
    {
        var product = await db.Products.FindAsync([request.ProductId], ct);
        if (product is null || product.IsDeleted)
            throw new NotFoundException($"Product '{request.ProductId}' not found.");

        if (!request.IsAdmin && product.SellerId != request.SellerId)
            throw new ForbiddenException("You do not own this product.");

        var existingCount = await db.ProductPhotos.CountAsync(p => p.ProductId == request.ProductId, ct);
        var available = MaxPhotosPerProduct - existingCount;

        if (available <= 0)
            throw new ConflictException($"Product already has {MaxPhotosPerProduct} photos.");

        if (request.Photos.Count > available)
            throw new ConflictException(
                $"Cannot upload {request.Photos.Count} photos: only {available} slot(s) left (max {MaxPhotosPerProduct}).");

        var hasMain = await db.ProductPhotos.AnyAsync(p => p.ProductId == request.ProductId && p.IsMain, ct);
        var nextSortOrder = existingCount;

        var addedIds = new List<Guid>(request.Photos.Count);

        for (var i = 0; i < request.Photos.Count; i++)
        {
            var file = request.Photos[i];

            if (file.Length > MaxFileSizeBytes)
                throw new ConflictException($"File '{file.FileName}' is too large. Maximum allowed size is 10 MB.");

            ProcessedImage processed;
            try
            {
                processed = await imageProcessor.ProcessAsync(file.OpenReadStream(), MaxWidth, MaxHeight, ct);
            }
            catch (UnknownImageFormatException)
            {
                throw new ConflictException($"File '{file.FileName}' is not a supported image format.");
            }
            catch (Exception ex) when (ex is InvalidImageContentException or ImageFormatException)
            {
                throw new ConflictException($"File '{file.FileName}' is corrupted or has invalid content.");
            }

            var photoId = Guid.NewGuid();
            var objectKey = $"products/{request.ProductId}/{photoId}.webp";

            using var stream = new MemoryStream(processed.Data);
            var storageUrl = await storageService.UploadAsync(stream, objectKey, ProcessedImage.MimeType, ct);

            var isMain = !hasMain && i == 0;
            if (isMain) hasMain = true;

            db.ProductPhotos.Add(new ProductPhoto(photoId)
            {
                ProductId = request.ProductId,
                StorageUrl = storageUrl,
                ObjectKey = objectKey,
                MimeType = ProcessedImage.MimeType,
                FileSize = processed.FileSize,
                SortOrder = nextSortOrder + i,
                IsMain = isMain,
                UploadedAt = DateTimeOffset.UtcNow
            });

            addedIds.Add(photoId);
        }

        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync($"product:{request.ProductId}", ct);

        return addedIds;
    }
}