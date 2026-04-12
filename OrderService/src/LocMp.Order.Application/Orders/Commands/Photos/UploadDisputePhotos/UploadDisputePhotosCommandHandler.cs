using AutoMapper;
using LocMp.BuildingBlocks.Application.Exceptions;
using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Order.Application.DTOs;
using LocMp.Order.Domain.Entities;
using LocMp.Order.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;

namespace LocMp.Order.Application.Orders.Commands.Photos.UploadDisputePhotos;

public sealed class UploadDisputePhotosCommandHandler(
    OrderDbContext db,
    IStorageService storageService,
    IImageProcessor imageProcessor,
    IMapper mapper)
    : IRequestHandler<UploadDisputePhotosCommand, IReadOnlyList<DisputePhotoDto>>
{
    private const int MaxWidth = 1200;
    private const int MaxHeight = 1200;
    private const int MaxPhotosPerDispute = 10;
    private const int MaxPhotosPerRequest = 5;

    public async Task<IReadOnlyList<DisputePhotoDto>> Handle(UploadDisputePhotosCommand request, CancellationToken ct)
    {
        var dispute = await db.Disputes
            .Include(d => d.Order)
            .Include(d => d.Photos)
            .FirstOrDefaultAsync(d => d.Id == request.DisputeId, ct)
            ?? throw new NotFoundException($"Dispute '{request.DisputeId}' not found.");

        if (!request.IsAdmin
            && dispute.Order.BuyerId != request.UploadedById
            && dispute.Order.SellerId != request.UploadedById)
            throw new ForbiddenException("You are not a participant in this dispute.");

        var available = MaxPhotosPerDispute - dispute.Photos.Count;

        if (available <= 0)
            throw new ConflictException($"Dispute already has {MaxPhotosPerDispute} photos.");

        if (request.Images.Count > MaxPhotosPerRequest)
            throw new ConflictException($"Cannot upload more than {MaxPhotosPerRequest} photos per request.");

        if (request.Images.Count > available)
            throw new ConflictException($"Cannot upload {request.Images.Count} photos: only {available} slot(s) left.");

        var nextSortOrder = dispute.Photos.Count;
        var added = new List<DisputePhoto>(request.Images.Count);

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
            var objectKey = $"disputes/{request.DisputeId}/photos/{photoId}.webp";

            using var stream = new MemoryStream(processed.Data);
            var storageUrl = await storageService.UploadAsync(stream, objectKey, ProcessedImage.MimeType, ct);

            var photo = new DisputePhoto(photoId)
            {
                DisputeId = request.DisputeId,
                UploadedById = request.UploadedById,
                StorageUrl = storageUrl,
                ObjectKey = objectKey,
                MimeType = ProcessedImage.MimeType,
                FileSize = processed.FileSize,
                SortOrder = nextSortOrder + i
            };

            db.DisputePhotos.Add(photo);
            added.Add(photo);
        }

        await db.SaveChangesAsync(ct);
        return added.Select(mapper.Map<DisputePhotoDto>).ToList();
    }
}
