using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Identity.Domain.Entities;
using LocMp.Identity.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace LocMp.Identity.Application.Identity.Commands.UserProfile.UploadUserPhoto;

public sealed class UploadUserPhotoCommandHandler(
    ApplicationDbContext dbContext,
    IStorageService storageService)
    : IRequestHandler<UploadUserPhotoCommand>
{
    private const int TargetSize = 400;
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
    private static readonly WebpEncoder Encoder = new() { Quality = 80 };
    private const string MimeType = "image/webp";

    public async Task Handle(UploadUserPhotoCommand request, CancellationToken ct)
    {
        if (request.Photo.Length > MaxFileSizeBytes)
            throw new ArgumentException("File is too large");

        byte[] processedData;
        try
        {
            processedData = await ProcessImageAsync(request.Photo.OpenReadStream(), ct);
        }
        catch (UnknownImageFormatException)
        {
            throw new InvalidOperationException("Uploaded file is not a supported image format.");
        }
        catch (Exception ex) when (ex is InvalidImageContentException or ImageFormatException)
        {
            throw new InvalidOperationException("Uploaded file is corrupted or has invalid content.");
        }

        var objectKey = $"photos/users/{request.UserId}.webp";

        using var stream = new MemoryStream(processedData);
        var storageUrl = await storageService.UploadAsync(stream, objectKey, MimeType, ct);

        var photo = await dbContext.UserPhotos
            .FirstOrDefaultAsync(p => p.UserId == request.UserId, ct);

        if (photo is null)
        {
            photo = new UserPhoto { Id = Guid.NewGuid(), UserId = request.UserId };
            dbContext.UserPhotos.Add(photo);
        }

        photo.StorageUrl = storageUrl;
        photo.ObjectKey = objectKey;
        photo.MimeType = MimeType;
        photo.FileSize = processedData.Length;
        photo.UploadedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);
    }

    private static async Task<byte[]> ProcessImageAsync(Stream stream, CancellationToken ct)
    {
        await using var inputStream = stream;
        using var image = await Image.LoadAsync(inputStream, ct);

        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(TargetSize, TargetSize),
            Mode = ResizeMode.Crop,
            Sampler = KnownResamplers.Lanczos3
        }));

        using var outputStream = new MemoryStream();
        await image.SaveAsync(outputStream, Encoder, ct);
        return outputStream.ToArray();
    }
}