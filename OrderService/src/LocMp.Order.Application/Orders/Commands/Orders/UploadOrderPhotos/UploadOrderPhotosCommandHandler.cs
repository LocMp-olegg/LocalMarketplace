using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Order.Application.DTOs;
using LocMp.Order.Domain.Entities;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;

namespace LocMp.Order.Application.Orders.Commands.Orders.UploadOrderPhotos;

public sealed class UploadOrderPhotosCommandHandler(
    OrderDbContext db,
    IStorageService storageService,
    IImageProcessor imageProcessor,
    IMapper mapper)
    : IRequestHandler<UploadOrderPhotosCommand, IReadOnlyList<OrderPhotoDto>>
{
    private const int MaxWidth = 1200;
    private const int MaxHeight = 1200;
    private const int MaxPhotosPerOrder = 10;
    private const int MaxPhotosPerRequest = 5;

    public async Task<IReadOnlyList<OrderPhotoDto>> Handle(UploadOrderPhotosCommand request, CancellationToken ct)
    {
        var order = await db.Orders.FindAsync([request.OrderId], ct)
                    ?? throw new NotFoundException($"Order '{request.OrderId}' not found.");

        if (!request.IsAdmin && order.BuyerId != request.UploadedById && order.SellerId != request.UploadedById)
            throw new ForbiddenException("You are not a participant in this order.");

        var existingCount = await db.OrderPhotos.CountAsync(p => p.OrderId == request.OrderId, ct);
        ValidatePhotoLimits(request.Images.Count, existingCount, MaxPhotosPerOrder, MaxPhotosPerRequest);

        var added = await UploadPhotosAsync(request, existingCount, ct);

        await db.SaveChangesAsync(ct);
        return added.Select(mapper.Map<OrderPhotoDto>).ToList();
    }

    private async Task<List<OrderPhoto>> UploadPhotosAsync(
        UploadOrderPhotosCommand request, int nextSortOrder, CancellationToken ct)
    {
        var added = new List<OrderPhoto>(request.Images.Count);

        for (var i = 0; i < request.Images.Count; i++)
        {
            var file = request.Images[i];
            var processed = await ProcessImageAsync(file, ct);

            var photoId = Guid.NewGuid();
            var objectKey = $"orders/{request.OrderId}/photos/{photoId}.webp";

            using var stream = new MemoryStream(processed.Data);
            var storageUrl = await storageService.UploadAsync(stream, objectKey, ProcessedImage.MimeType, ct);

            var photo = new OrderPhoto(photoId)
            {
                OrderId = request.OrderId,
                UploadedById = request.UploadedById,
                StorageUrl = storageUrl,
                ObjectKey = objectKey,
                MimeType = ProcessedImage.MimeType,
                FileSize = processed.FileSize,
                SortOrder = nextSortOrder + i
            };

            db.OrderPhotos.Add(photo);
            added.Add(photo);
        }

        return added;
    }

    private async Task<ProcessedImage> ProcessImageAsync(IFormFile file, CancellationToken ct)
    {
        try
        {
            return await imageProcessor.ProcessAsync(file.OpenReadStream(), MaxWidth, MaxHeight, ct);
        }
        catch (UnknownImageFormatException)
        {
            throw new ConflictException($"File '{file.FileName}' is not a supported image format.");
        }
        catch (Exception ex) when (ex is InvalidImageContentException or ImageFormatException)
        {
            throw new ConflictException($"File '{file.FileName}' is corrupted or has invalid content.");
        }
    }

    private static void ValidatePhotoLimits(int incoming, int existing, int maxTotal, int maxPerRequest)
    {
        var available = maxTotal - existing;

        if (available <= 0)
            throw new ConflictException($"Order already has {maxTotal} photos.");

        if (incoming > maxPerRequest)
            throw new ConflictException($"Cannot upload more than {maxPerRequest} photos per request.");

        if (incoming > available)
            throw new ConflictException($"Cannot upload {incoming} photos: only {available} slot(s) left.");
    }
}