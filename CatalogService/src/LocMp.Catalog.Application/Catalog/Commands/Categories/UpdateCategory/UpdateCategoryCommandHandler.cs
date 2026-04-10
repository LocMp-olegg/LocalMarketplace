using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using SixLabors.ImageSharp;

namespace LocMp.Catalog.Application.Catalog.Commands.Categories.UpdateCategory;

public sealed class UpdateCategoryCommandHandler(
    CatalogDbContext db,
    IMapper mapper,
    IStorageService storageService,
    IImageProcessor imageProcessor,
    IDistributedCache cache)
    : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    private const int MaxSize = 800;
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken ct)
    {
        var category = await db.Categories.FindAsync([request.Id], ct)
                       ?? throw new NotFoundException($"Category '{request.Id}' not found.");

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

            var objectKey = $"categories/{request.Id}.webp";
            using var stream = new MemoryStream(processed.Data);
            category.ImageUrl = await storageService.UploadAsync(stream, objectKey, ProcessedImage.MimeType, ct);
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.SortOrder = request.SortOrder;
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
        await cache.RemoveAsync("categories:all", ct);
        await cache.RemoveAsync($"category:{request.Id}", ct);

        return mapper.Map<CategoryDto>(category);
    }
}
