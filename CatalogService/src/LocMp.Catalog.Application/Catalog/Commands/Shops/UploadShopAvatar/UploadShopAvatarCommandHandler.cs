using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;

namespace LocMp.Catalog.Application.Catalog.Commands.Shops.UploadShopAvatar;

public sealed class UploadShopAvatarCommandHandler(
    CatalogDbContext db,
    IMapper mapper,
    IStorageService storageService,
    IImageProcessor imageProcessor)
    : IRequestHandler<UploadShopAvatarCommand, ShopDto>
{
    private const int MaxSize = 800;
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    public async Task<ShopDto> Handle(UploadShopAvatarCommand request, CancellationToken ct)
    {
        if (request.Image.Length > MaxFileSizeBytes)
            throw new ConflictException("File is too large. Maximum allowed size is 5 MB.");

        var shop = await db.Shops
                       .Include(s => s.Photos)
                       .FirstOrDefaultAsync(s => s.Id == request.ShopId, ct)
                   ?? throw new NotFoundException($"Shop '{request.ShopId}' not found.");

        if (!request.IsAdmin && shop.SellerId != request.RequesterId)
            throw new ForbiddenException("You can only upload avatar for your own shop.");

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

        var objectKey = $"shops/{request.ShopId}/avatar.webp";
        using var stream = new MemoryStream(processed.Data);
        shop.AvatarUrl = await storageService.UploadAsync(stream, objectKey, ProcessedImage.MimeType, ct);
        shop.AvatarObjectKey = objectKey;
        shop.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
        return mapper.Map<ShopDto>(shop);
    }
}
