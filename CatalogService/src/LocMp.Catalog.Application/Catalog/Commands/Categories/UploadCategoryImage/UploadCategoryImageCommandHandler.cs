using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace LocMp.Catalog.Application.Catalog.Commands.Categories.UploadCategoryImage;

public sealed class UploadCategoryImageCommandHandler(
    CatalogDbContext db,
    IStorageService storageService,
    IDistributedCache cache)
    : IRequestHandler<UploadCategoryImageCommand, string>
{
    private const int MaxSize = 800;
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
    private static readonly WebpEncoder Encoder = new() { Quality = 85 };
    private const string MimeType = "image/webp";

    public async Task<string> Handle(UploadCategoryImageCommand request, CancellationToken ct)
    {
        if (request.Image.Length > MaxFileSizeBytes)
            throw new ConflictException("File is too large. Maximum allowed size is 5 MB.");

        var category = await db.Categories.FindAsync([request.CategoryId], ct)
                       ?? throw new NotFoundException($"Category '{request.CategoryId}' not found.");

        byte[] processedData;
        try
        {
            processedData = await ProcessImageAsync(request.Image.OpenReadStream(), ct);
        }
        catch (UnknownImageFormatException)
        {
            throw new ConflictException("Uploaded file is not a supported image format.");
        }
        catch (Exception ex) when (ex is InvalidImageContentException or ImageFormatException)
        {
            throw new ConflictException("Uploaded file is corrupted or has invalid content.");
        }

        // Фиксированный ключ — при повторной загрузке перезаписывает объект в MinIO
        var objectKey = $"categories/{request.CategoryId}.webp";

        using var stream = new MemoryStream(processedData);
        var storageUrl = await storageService.UploadAsync(stream, objectKey, MimeType, ct);

        category.ImageUrl = storageUrl;
        category.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync("categories:all", ct);
        await cache.RemoveAsync($"category:{request.CategoryId}", ct);

        return storageUrl;
    }

    private static async Task<byte[]> ProcessImageAsync(Stream stream, CancellationToken ct)
    {
        await using var inputStream = stream;
        using var image = await Image.LoadAsync(inputStream, ct);

        if (image.Width > MaxSize || image.Height > MaxSize)
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(MaxSize, MaxSize),
                Mode = ResizeMode.Max,
                Sampler = KnownResamplers.Lanczos3
            }));
        }

        using var outputStream = new MemoryStream();
        await image.SaveAsync(outputStream, Encoder, ct);
        return outputStream.ToArray();
    }
}