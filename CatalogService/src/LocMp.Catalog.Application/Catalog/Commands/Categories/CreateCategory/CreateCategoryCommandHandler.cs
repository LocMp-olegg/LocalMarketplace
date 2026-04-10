using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Domain.Entities;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using SixLabors.ImageSharp;

namespace LocMp.Catalog.Application.Catalog.Commands.Categories.CreateCategory;

public sealed class CreateCategoryCommandHandler(
    CatalogDbContext db,
    IMapper mapper,
    IStorageService storageService,
    IImageProcessor imageProcessor,
    IDistributedCache cache)
    : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private const int MaxSize = 800;
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

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

            ProcessedImage processed;
            try
            {
                processed = await imageProcessor.ProcessAsync(request.Image.OpenReadStream(), MaxSize, MaxSize, ct);
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
            using var stream = new MemoryStream(processed.Data);
            imageUrl = await storageService.UploadAsync(stream, objectKey, ProcessedImage.MimeType, ct);
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
        await cache.RemoveAsync("categories:all", ct);

        return mapper.Map<CategoryDto>(category);
    }
}
