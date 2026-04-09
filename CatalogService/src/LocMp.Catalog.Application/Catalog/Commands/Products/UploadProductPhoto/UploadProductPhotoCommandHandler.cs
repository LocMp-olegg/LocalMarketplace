using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Catalog.Domain.Entities;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace LocMp.Catalog.Application.Catalog.Commands.Products.UploadProductPhoto;

public sealed class UploadProductPhotoCommandHandler(
    CatalogDbContext db,
    IStorageService storageService)
    : IRequestHandler<UploadProductPhotoCommand, Guid>
{
    private const int MaxWidth = 1200;
    private const int MaxHeight = 1200;
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;
    private static readonly WebpEncoder Encoder = new() { Quality = 85 };
    private const string MimeType = "image/webp";

    public async Task<Guid> Handle(UploadProductPhotoCommand request, CancellationToken ct)
    {
        if (request.Photo.Length > MaxFileSizeBytes)
            throw new ConflictException("File is too large. Maximum allowed size is 10 MB.");

        var product = await db.Products.FindAsync([request.ProductId], ct)
                      ?? throw new NotFoundException($"Product '{request.ProductId}' not found.");

        if (!request.IsAdmin && product.SellerId != request.SellerId)
            throw new ForbiddenException("You do not own this product.");

        byte[] processedData;
        try
        {
            processedData = await ProcessImageAsync(request.Photo.OpenReadStream(), ct);
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
        var objectKey = $"products/{request.ProductId}/{photoId}.webp";

        using var stream = new MemoryStream(processedData);
        var storageUrl = await storageService.UploadAsync(stream, objectKey, MimeType, ct);

        if (request.IsMain)
        {
            var existing = await db.ProductPhotos
                .Where(p => p.ProductId == request.ProductId && p.IsMain)
                .ToListAsync(ct);
            existing.ForEach(p => p.IsMain = false);
        }

        var photo = new ProductPhoto(photoId)
        {
            ProductId = request.ProductId,
            StorageUrl = storageUrl,
            ObjectKey = objectKey,
            MimeType = MimeType,
            FileSize = processedData.Length,
            SortOrder = request.SortOrder,
            IsMain = request.IsMain,
            UploadedAt = DateTimeOffset.UtcNow
        };

        db.ProductPhotos.Add(photo);
        await db.SaveChangesAsync(ct);

        return photoId;
    }

    private static async Task<byte[]> ProcessImageAsync(Stream stream, CancellationToken ct)
    {
        await using var inputStream = stream;
        using var image = await Image.LoadAsync(inputStream, ct);

        if (image.Width > MaxWidth || image.Height > MaxHeight)
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(MaxWidth, MaxHeight),
                Mode = ResizeMode.Max,
                Sampler = KnownResamplers.Lanczos3
            }));
        }

        using var outputStream = new MemoryStream();
        await image.SaveAsync(outputStream, Encoder, ct);
        return outputStream.ToArray();
    }
}
