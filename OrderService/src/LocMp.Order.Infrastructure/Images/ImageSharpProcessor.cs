using LocMp.BuildingBlocks.Application.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace LocMp.Order.Infrastructure.Images;

public sealed class ImageSharpProcessor : IImageProcessor
{
    private static readonly WebpEncoder Encoder = new() { Quality = 85 };

    public async Task<ProcessedImage> ProcessAsync(Stream input, int maxWidth, int maxHeight,
        CancellationToken ct = default)
    {
        await using var inputStream = input;
        using var image = await Image.LoadAsync(inputStream, ct);

        if (image.Width > maxWidth || image.Height > maxHeight)
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(maxWidth, maxHeight),
                Mode = ResizeMode.Max,
                Sampler = KnownResamplers.Lanczos3
            }));
        }

        using var output = new MemoryStream();
        await image.SaveAsync(output, Encoder, ct);
        var data = output.ToArray();
        return new ProcessedImage(data, data.Length);
    }
}