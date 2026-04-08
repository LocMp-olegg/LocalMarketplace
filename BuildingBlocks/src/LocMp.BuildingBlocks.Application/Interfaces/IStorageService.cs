namespace LocMp.BuildingBlocks.Application.Interfaces;

public interface IStorageService
{
    /// <summary>Загружает файл в хранилище и возвращает публичный URL.</summary>
    Task<string> UploadAsync(Stream stream, string objectKey, string contentType, CancellationToken ct = default);

    /// <summary>Удаляет файл из хранилища.</summary>
    Task DeleteAsync(string objectKey, CancellationToken ct = default);

    /// <summary>Возвращает публичный URL для существующего объекта.</summary>
    string GetUrl(string objectKey);
}