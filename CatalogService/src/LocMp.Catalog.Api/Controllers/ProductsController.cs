using LocMp.BuildingBlocks.Application.Common;
using LocMp.Catalog.Api.Filters;
using LocMp.Catalog.Api.Requests.Products;
using LocMp.Catalog.Application.Catalog.Commands.Products.AddProductTag;
using LocMp.Catalog.Application.Catalog.Commands.Products.CreateProduct;
using LocMp.Catalog.Application.Catalog.Commands.Products.DeleteProduct;
using LocMp.Catalog.Application.Catalog.Commands.Products.DeleteProductPhoto;
using LocMp.Catalog.Application.Catalog.Commands.Products.ReleaseStock;
using LocMp.Catalog.Application.Catalog.Commands.Products.RemoveProductTag;
using LocMp.Catalog.Application.Catalog.Commands.Products.ReserveStock;
using LocMp.Catalog.Application.Catalog.Commands.Products.UpdateProduct;
using LocMp.Catalog.Application.Catalog.Commands.Products.UpdateStock;
using LocMp.Catalog.Application.Catalog.Commands.Products.SetMainProductPhoto;
using LocMp.Catalog.Application.Catalog.Commands.Products.UploadProductPhoto;
using LocMp.Catalog.Application.Catalog.Queries.Products.GetProductById;
using LocMp.Catalog.Application.Catalog.Queries.Products.GetMyProducts;
using LocMp.Catalog.Application.Catalog.Queries.Products.GetProductsByLocation;
using LocMp.Catalog.Application.Catalog.Queries.Products.GetProductsBySeller;
using LocMp.Catalog.Application.Catalog.Queries.Products.GetProductsByShop;
using LocMp.Catalog.Application.Catalog.Queries.Products.GetProductsByTags;
using LocMp.Catalog.Application.Catalog.Queries.Products.GetProductStock;
using LocMp.Catalog.Application.Catalog.Queries.Products.GetStockHistory;
using LocMp.Catalog.Application.Catalog.Queries.Products.SearchProducts;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Api.Extensions;
using LocMp.Catalog.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocMp.Catalog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController(ISender sender) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken ct)
    {
        var viewerId = HttpContext.User.Identity?.IsAuthenticated == true
            ? HttpContext.GetUserId()
            : (Guid?)null;
        return Ok(await sender.Send(new GetProductByIdQuery(id, viewerId), ct));
    }

    [HttpGet("nearby")]
    public async Task<ActionResult<PagedResult<ProductSummaryDto>>> GetNearby(
        [FromQuery] double lat,
        [FromQuery] double lon,
        [FromQuery] double radiusKm = 5,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? search = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] bool isInStock = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await sender.Send(
            new GetProductsByLocationQuery(lat, lon, radiusKm, categoryId, search, minPrice, maxPrice, isInStock, page, pageSize), ct));

    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<ProductSummaryDto>>> Search(
        [FromQuery(Name = "search")] string? q = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? tags = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] Guid? shopId = null,
        [FromQuery] bool isInStock = false,
        [FromQuery] ProductSortBy sort = ProductSortBy.Newest,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var tagList = string.IsNullOrWhiteSpace(tags)
            ? null
            : tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return Ok(await sender.Send(
            new SearchProductsQuery(q, categoryId, tagList, minPrice, maxPrice, shopId, isInStock, sort, page, pageSize), ct));
    }

    [HttpGet("by-seller/{sellerId:guid}")]
    public async Task<ActionResult<PagedResult<ProductSummaryDto>>> GetBySeller(
        Guid sellerId,
        [FromQuery] Guid? shopId = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] bool isInStock = false,
        [FromQuery] bool includeInactive = false,
        [FromQuery] ProductSortBy sort = ProductSortBy.Newest,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await sender.Send(
            new GetProductsBySellerQuery(sellerId, shopId, categoryId, minPrice, maxPrice, isInStock, includeInactive, sort, page, pageSize), ct));

    [HttpGet("by-shop/{shopId:guid}")]
    public async Task<ActionResult<PagedResult<ProductSummaryDto>>> GetByShop(
        Guid shopId,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? search = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] bool isInStock = false,
        [FromQuery] ProductSortBy sort = ProductSortBy.Newest,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await sender.Send(
            new GetProductsByShopQuery(shopId, categoryId, search, minPrice, maxPrice, isInStock, sort, page, pageSize), ct));

    [HttpGet("my")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Seller,Admin")]
    public async Task<ActionResult<PagedResult<ProductSummaryDto>>> GetMy(
        [FromQuery] Guid? shopId = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? search = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] bool isInStock = false,
        [FromQuery] ProductSortBy sort = ProductSortBy.Newest,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await sender.Send(
            new GetMyProductsQuery(HttpContext.GetUserId(), shopId, categoryId, search, minPrice, maxPrice, isActive, isInStock, sort, page, pageSize), ct));

    [HttpGet("by-tags")]
    public async Task<ActionResult<PagedResult<ProductSummaryDto>>> GetByTags(
        [FromQuery] string tags,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var tagList = tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return Ok(await sender.Send(new GetProductsByTagsQuery(tagList, page, pageSize), ct));
    }

    [HttpGet("{id:guid}/stock")]
    public async Task<ActionResult<int>> GetStock(Guid id, CancellationToken ct)
        => Ok(await sender.Send(new GetProductStockQuery(id), ct));

    [HttpGet("{id:guid}/stock/history")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Seller,Admin")]
    public async Task<ActionResult<PagedResult<StockHistoryDto>>> GetStockHistory(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetStockHistoryQuery(
            id, HttpContext.GetUserId(), HttpContext.User.IsInRole("Admin"), page, pageSize), ct));

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Seller,Admin")]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var command = new CreateProductCommand(
            HttpContext.GetUserId(),
            request.ShopId,
            request.CategoryId,
            request.Name,
            request.Description,
            request.Price,
            request.Unit,
            request.InitialStock,
            request.Latitude,
            request.Longitude
        );
        var result = await sender.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Seller,Admin")]
    public async Task<ActionResult<ProductDto>> Update(Guid id, [FromBody] UpdateProductRequest request,
        CancellationToken ct)
    {
        var command = new UpdateProductCommand(
            id,
            HttpContext.GetUserId(),
            request.CategoryId,
            request.Name,
            request.Description,
            request.Price,
            request.Unit,
            request.Latitude,
            request.Longitude,
            request.IsActive
        );
        return Ok(await sender.Send(command, ct));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Seller,Admin")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeleteProductCommand(id, HttpContext.GetUserId()), ct);
        return NoContent();
    }

    [HttpPatch("{id:guid}/stock")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Seller,Admin")]
    public async Task<ActionResult<int>> UpdateStock(
        Guid id, [FromBody] UpdateStockRequest request, CancellationToken ct)
    {
        var command = new UpdateStockCommand(
            id,
            HttpContext.GetUserId(),
            request.QuantityDelta,
            request.ChangeType,
            request.ReferenceId
        );
        return Ok(await sender.Send(command, ct));
    }

    [HttpPost("{id:guid}/reserve")]
    [InternalApiKey]
    public async Task<ActionResult> Reserve(Guid id, [FromBody] ReserveStockRequest request, CancellationToken ct)
    {
        await sender.Send(new ReserveStockCommand(id, request.Quantity, request.OrderId), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/release")]
    [InternalApiKey]
    public async Task<ActionResult> Release(Guid id, [FromBody] ReleaseStockRequest request, CancellationToken ct)
    {
        await sender.Send(new ReleaseStockCommand(id, request.Quantity, request.OrderId), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/photos")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Seller,Admin")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<IReadOnlyList<Guid>>> UploadPhotos(
        Guid id,
        IFormFileCollection photos,
        CancellationToken ct = default)
    {
        var ids = await sender.Send(
            new UploadProductPhotoCommand(id, HttpContext.GetUserId(), photos.ToList(),
                IsAdmin: HttpContext.User.IsInRole("Admin")), ct);
        return CreatedAtAction(nameof(GetById), new { id }, ids);
        
    }

    [HttpDelete("{productId:guid}/photos/{photoId:guid}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Seller,Admin")]
    public async Task<ActionResult> DeletePhoto(Guid productId, Guid photoId, CancellationToken ct)
    {
        await sender.Send(new DeleteProductPhotoCommand(photoId, HttpContext.GetUserId(),
            IsAdmin: HttpContext.User.IsInRole("Admin")), ct);
        return NoContent();
    }

    [HttpPatch("{productId:guid}/photos/{photoId:guid}/set-main")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Seller,Admin")]
    public async Task<ActionResult> SetMainPhoto(Guid productId, Guid photoId, CancellationToken ct)
    {
        await sender.Send(new SetMainProductPhotoCommand(
            photoId, productId, HttpContext.GetUserId(), HttpContext.User.IsInRole("Admin")), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/tags")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Seller,Admin")]
    public async Task<ActionResult<TagDto>> AddTag(
        Guid id, [FromBody] AddProductTagRequest request, CancellationToken ct)
        => Ok(await sender.Send(new AddProductTagCommand(
            id, request.TagName, HttpContext.GetUserId(), HttpContext.User.IsInRole("Admin")), ct));

    [HttpDelete("{id:guid}/tags/{tagId:guid}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Seller,Admin")]
    public async Task<ActionResult> RemoveTag(Guid id, Guid tagId, CancellationToken ct)
    {
        await sender.Send(new RemoveProductTagCommand(
            id, tagId, HttpContext.GetUserId(), HttpContext.User.IsInRole("Admin")), ct);
        return NoContent();
    }
}