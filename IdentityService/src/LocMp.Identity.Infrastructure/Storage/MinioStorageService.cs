using LocMp.BuildingBlocks.Application.Interfaces;
using LocMp.Identity.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace LocMp.Identity.Infrastructure.Storage;

public sealed class MinioStorageService(IMinioClient minioClient, IOptions<MinioOptions> options) : IStorageService
{
    private readonly MinioOptions _opts = options.Value;

    public async Task<string> UploadAsync(Stream stream, string objectKey, string contentType,
        CancellationToken ct = default)
    {
        await EnsureBucketExistsAsync(ct);

        var args = new PutObjectArgs()
            .WithBucket(_opts.BucketName)
            .WithObject(objectKey)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(contentType);

        await minioClient.PutObjectAsync(args, ct);

        return GetUrl(objectKey);
    }

    public async Task DeleteAsync(string objectKey, CancellationToken ct = default)
    {
        var args = new RemoveObjectArgs()
            .WithBucket(_opts.BucketName)
            .WithObject(objectKey);

        await minioClient.RemoveObjectAsync(args, ct);
    }

    public string GetUrl(string objectKey)
    {
        var scheme = _opts.UseSSL ? "https" : "http";
        return $"{scheme}://{_opts.PublicEndpoint}/{_opts.BucketName}/{objectKey}";
    }

    private async Task EnsureBucketExistsAsync(CancellationToken ct)
    {
        var exists = await minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(_opts.BucketName), ct);

        if (!exists)
        {
            await minioClient.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(_opts.BucketName), ct);

            // Открываем публичный доступ на чтение (GET) для всех объектов бакета
            var policy = $$"""
                           {
                               "Version": "2012-10-17",
                               "Statement": [{
                                   "Effect": "Allow",
                                   "Principal": { "AWS": ["*"] },
                                   "Action": ["s3:GetObject"],
                                   "Resource": ["arn:aws:s3:::{{_opts.BucketName}}/*"]
                               }]
                           }
                           """;

            await minioClient.SetPolicyAsync(
                new SetPolicyArgs().WithBucket(_opts.BucketName).WithPolicy(policy), ct);
        }
    }
}