using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Catalog.Application.Catalog.Commands.Categories.CreateCategory;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace LocMp.Catalog.Application.Catalog.Commands.Categories.UpdateCategory;

public sealed class UpdateCategoryCommandHandler(CatalogDbContext db, IStorageService storageService)
    : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    private const int MaxSize = 800;
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
    private static readonly WebpEncoder Encoder = new() { Quality = 85 };
    private const string MimeType = "image/webp";

    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken ct)
    {
        var category = await db.Categories.FindAsync([request.Id], ct)
                       ?? throw new NotFoundException($"Category '{request.Id}' not found.");

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

            var objectKey = $"categories/{request.Id}.webp";
            using var stream = new MemoryStream(processedData);
            category.ImageUrl = await storageService.UploadAsync(stream, objectKey, MimeType, ct);
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.SortOrder = request.SortOrder;
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        return CreateCategoryCommandHandler.ToDto(category);
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