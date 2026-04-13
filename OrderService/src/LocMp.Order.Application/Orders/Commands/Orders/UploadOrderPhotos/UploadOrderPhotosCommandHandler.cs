using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Order.Application.DTOs;
using LocMp.Order.Domain.Entities;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
        var available = MaxPhotosPerOrder - existingCount;

        if (available <= 0)
            throw new ConflictException($"Order already has {MaxPhotosPerOrder} photos.");

        if (request.Images.Count > MaxPhotosPerRequest)
            throw new ConflictException($"Cannot upload more than {MaxPhotosPerRequest} photos per request.");

        if (request.Images.Count > available)
            throw new ConflictException($"Cannot upload {request.Images.Count} photos: only {available} slot(s) left.");

        var nextSortOrder = existingCount;
        var added = new List<OrderPhoto>(request.Images.Count);

        for (var i = 0; i < request.Images.Count; i++)
        {
            var file = request.Images[i];

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

        await db.SaveChangesAsync(ct);
        return added.Select(mapper.Map<OrderPhotoDto>).ToList();
    }
}