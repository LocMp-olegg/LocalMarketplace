using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Domain.Entities;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;

namespace LocMp.Catalog.Application.Catalog.Commands.Shops.UploadShopPhoto;

public sealed class UploadShopPhotoCommandHandler(
    CatalogDbContext db,
    IMapper mapper,
    IStorageService storageService,
    IImageProcessor imageProcessor)
    : IRequestHandler<UploadShopPhotoCommand, IReadOnlyList<ShopPhotoDto>>
{
    private const int MaxWidth = 1200;
    private const int MaxHeight = 1200;
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;
    private const int MaxPhotosPerShop = 50;
    private const int MaxPhotosPerRequest = 10;

    public async Task<IReadOnlyList<ShopPhotoDto>> Handle(UploadShopPhotoCommand request, CancellationToken ct)
    {
        var shop = await db.Shops.FindAsync([request.ShopId], ct)
                   ?? throw new NotFoundException($"Shop '{request.ShopId}' not found.");

        if (!request.IsAdmin && shop.SellerId != request.RequesterId)
            throw new ForbiddenException("You can only upload photos for your own shop.");

        var existingCount = await db.ShopPhotos.CountAsync(p => p.ShopId == request.ShopId, ct);
        var available = MaxPhotosPerShop - existingCount;

        if (available <= 0)
            throw new ConflictException($"Shop already has {MaxPhotosPerShop} photos.");

        if (request.Images.Count > MaxPhotosPerRequest)
            throw new ConflictException($"Cannot upload more than {MaxPhotosPerRequest} photos per request.");

        if (request.Images.Count > available)
            throw new ConflictException(
                $"Cannot upload {request.Images.Count} photos: only {available} slot(s) left (max {MaxPhotosPerShop}).");

        var nextSortOrder = existingCount;
        var added = new List<ShopPhoto>(request.Images.Count);

        for (var i = 0; i < request.Images.Count; i++)
        {
            var file = request.Images[i];

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
            var objectKey = $"shops/{request.ShopId}/photos/{photoId}.webp";

            using var stream = new MemoryStream(processed.Data);
            var storageUrl = await storageService.UploadAsync(stream, objectKey, ProcessedImage.MimeType, ct);

            var photo = new ShopPhoto(photoId)
            {
                ShopId = request.ShopId,
                StorageUrl = storageUrl,
                ObjectKey = objectKey,
                MimeType = ProcessedImage.MimeType,
                FileSize = processed.FileSize,
                SortOrder = nextSortOrder + i,
                UploadedAt = DateTimeOffset.UtcNow
            };

            db.ShopPhotos.Add(photo);
            added.Add(photo);
        }

        await db.SaveChangesAsync(ct);
        return added.Select(mapper.Map<ShopPhotoDto>).ToList();
    }
}