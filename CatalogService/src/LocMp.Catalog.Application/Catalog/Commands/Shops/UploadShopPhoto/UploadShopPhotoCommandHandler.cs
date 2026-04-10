using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Domain.Entities;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using SixLabors.ImageSharp;

namespace LocMp.Catalog.Application.Catalog.Commands.Shops.UploadShopPhoto;

public sealed class UploadShopPhotoCommandHandler(
    CatalogDbContext db,
    IMapper mapper,
    IStorageService storageService,
    IImageProcessor imageProcessor)
    : IRequestHandler<UploadShopPhotoCommand, ShopPhotoDto>
{
    private const int MaxWidth = 1200;
    private const int MaxHeight = 1200;
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    public async Task<ShopPhotoDto> Handle(UploadShopPhotoCommand request, CancellationToken ct)
    {
        if (request.Image.Length > MaxFileSizeBytes)
            throw new ConflictException("File is too large. Maximum allowed size is 10 MB.");

        var shop = await db.Shops.FindAsync([request.ShopId], ct)
                   ?? throw new NotFoundException($"Shop '{request.ShopId}' not found.");

        if (!request.IsAdmin && shop.SellerId != request.RequesterId)
            throw new ForbiddenException("You can only upload photos for your own shop.");

        ProcessedImage processed;
        try
        {
            processed = await imageProcessor.ProcessAsync(request.Image.OpenReadStream(), MaxWidth, MaxHeight, ct);
        }
        catch (UnknownImageFormatException)
        {
            throw new ConflictException("Uploaded file is not a supported image format.");
        }
        catch (Exception ex) when (ex is InvalidImageContentException or ImageFormatException)
        {
            throw new ConflictException("Uploaded file is corrupted or has invalid content.");
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
            SortOrder = request.SortOrder,
            UploadedAt = DateTimeOffset.UtcNow
        };

        db.ShopPhotos.Add(photo);
        await db.SaveChangesAsync(ct);
        return mapper.Map<ShopPhotoDto>(photo);
    }
}
