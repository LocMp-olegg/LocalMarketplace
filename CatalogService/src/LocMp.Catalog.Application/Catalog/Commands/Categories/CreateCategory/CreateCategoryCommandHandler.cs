using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Domain.Entities;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace LocMp.Catalog.Application.Catalog.Commands.Categories.CreateCategory;

public sealed class CreateCategoryCommandHandler(CatalogDbContext db, IStorageService storageService)
    : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private const int MaxSize = 800;
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
    private static readonly WebpEncoder Encoder = new() { Quality = 85 };
    private const string MimeType = "image/webp";

    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        if (request.ParentCategoryId.HasValue)
        {
            var parentExists = await db.Categories.AnyAsync(c => c.Id == request.ParentCategoryId.Value, ct);
            if (!parentExists)
                throw new NotFoundException($"Parent category '{request.ParentCategoryId}' not found.");
        }

        var categoryId = Guid.NewGuid();
        string? imageUrl = null;

        if (request.Image is not null)
        {
            if (request.Image.Length > MaxFileSizeBytes)
                throw new ConflictException("File is too large. Maximum allowed size is 5 MB.");

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

            var objectKey = $"categories/{categoryId}.webp";
            using var stream = new MemoryStream(processedData);
            imageUrl = await storageService.UploadAsync(stream, objectKey, MimeType, ct);
        }

        var category = new Category(categoryId)
        {
            ParentCategoryId = request.ParentCategoryId,
            Name = request.Name,
            Description = request.Description,
            ImageUrl = imageUrl,
            SortOrder = request.SortOrder,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);

        return ToDto(category);
    }

    internal static CategoryDto ToDto(Category c) => new(
        c.Id, c.ParentCategoryId, c.Name, c.Description,
        c.ImageUrl, c.SortOrder, c.IsActive, c.CreatedAt);

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
