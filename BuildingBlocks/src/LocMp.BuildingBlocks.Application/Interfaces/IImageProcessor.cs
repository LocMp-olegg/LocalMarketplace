namespace LocMp.BuildingBlocks.Application.Interfaces;

public interface IImageProcessor
{
    /// <summary>
    /// Resizes the image to fit within maxWidth×maxHeight and converts it to WebP.
    /// </summary>
    Task<ProcessedImage> ProcessAsync(Stream input, int maxWidth, int maxHeight, CancellationToken ct = default);
}

public sealed record ProcessedImage(byte[] Data, long FileSize)
{
    public const string MimeType = "image/webp";
}