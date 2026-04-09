using LocMp.BuildingBlocks.Application.Common;
using LocMp.Catalog.Api.Requests.Products;
using LocMp.Catalog.Application.Catalog.Commands.Products.CreateProduct;
using LocMp.Catalog.Application.Catalog.Commands.Products.DeleteProduct;
using LocMp.Catalog.Application.Catalog.Commands.Products.DeleteProductPhoto;
using LocMp.Catalog.Application.Catalog.Commands.Products.UpdateProduct;
using LocMp.Catalog.Application.Catalog.Commands.Products.UpdateStock;
using LocMp.Catalog.Application.Catalog.Commands.Products.UploadProductPhoto;
using LocMp.Catalog.Application.Catalog.Queries.Products.GetProductById;
using LocMp.Catalog.Application.Catalog.Queries.Products.GetProductsByLocation;
using LocMp.Catalog.Application.Catalog.Queries.Products.GetProductsBySeller;
using LocMp.Catalog.Application.Catalog.Queries.Products.GetProductsByTags;
using LocMp.Catalog.Application.Catalog.Queries.Products.GetProductStock;
using LocMp.Catalog.Application.DTOs;
using LocMp.Catalog.Infrastructure.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocMp.Catalog.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(ISender sender) : ControllerBase
{
    // ── Public queries ──────────────────────────────────────────────────────

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
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await sender.Send(
            new GetProductsByLocationQuery(lat, lon, radiusKm, categoryId, search, page, pageSize), ct));

    [HttpGet("by-seller/{sellerId:guid}")]
    public async Task<ActionResult<PagedResult<ProductSummaryDto>>> GetBySeller(
        Guid sellerId,
        [FromQuery] bool includeInactive = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetProductsBySellerQuery(sellerId, includeInactive, page, pageSize), ct));

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

    // ── Seller-only commands ────────────────────────────────────────────────

    [HttpPost]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Seller,Admin")]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var command = new CreateProductCommand(
            HttpContext.GetUserId(),
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
    public async Task<ActionResult<ProductDto>> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct)
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

    [HttpPost("{id:guid}/photos")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Seller,Admin")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<Guid>> UploadPhoto(
        Guid id,
        IFormFile photo,
        [FromQuery] bool isMain = false,
        [FromQuery] int sortOrder = 0,
        CancellationToken ct = default)
    {
        var photoId = await sender.Send(
            new UploadProductPhotoCommand(id, HttpContext.GetUserId(), photo, isMain, sortOrder,
                IsAdmin: HttpContext.User.IsInRole("Admin")), ct);
        return CreatedAtAction(nameof(GetById), new { id }, photoId);
    }

    [HttpDelete("{productId:guid}/photos/{photoId:guid}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Seller,Admin")]
    public async Task<ActionResult> DeletePhoto(Guid productId, Guid photoId, CancellationToken ct)
    {
        await sender.Send(new DeleteProductPhotoCommand(photoId, HttpContext.GetUserId(),
            IsAdmin: HttpContext.User.IsInRole("Admin")), ct);
        return NoContent();
    }
}
