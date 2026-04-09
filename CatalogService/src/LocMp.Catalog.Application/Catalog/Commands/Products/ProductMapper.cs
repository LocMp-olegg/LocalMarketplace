using LocMp.Catalog.Application.DTOs;

namespace LocMp.Catalog.Application.Catalog.Commands.Products;

internal static class ProductMapper
{
    internal static ProductDto ToDto(Domain.Entities.Product p) => new(
        p.Id,
        p.SellerId,
        p.ShopId,
        p.CategoryId,
        p.Name,
        p.Description,
        p.Price,
        p.Unit,
        p.StockQuantity,
        p.Location?.Y,
        p.Location?.X,
        p.IsActive,
        p.CreatedAt,
        p.UpdatedAt,
        p.Photos.FirstOrDefault(ph => ph.IsMain)?.StorageUrl
            ?? p.Photos.OrderBy(ph => ph.SortOrder).FirstOrDefault()?.StorageUrl,
        p.Photos.OrderBy(ph => ph.SortOrder)
            .Select(ph => new ProductPhotoDto(ph.Id, ph.ProductId, ph.StorageUrl, ph.MimeType, ph.SortOrder, ph.IsMain, ph.UploadedAt))
            .ToList(),
        p.ProductTags.Select(pt => pt.Tag.Name).ToList()
    );
}
