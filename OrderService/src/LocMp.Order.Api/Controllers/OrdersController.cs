using LocMp.BuildingBlocks.Application.Common;
using LocMp.BuildingBlocks.Infrastructure.Extensions;
using LocMp.Order.Api.Requests;
using LocMp.Order.Application.DTOs;
using LocMp.Order.Application.Orders.Commands.Orders.AssignCourier;
using LocMp.Order.Application.Orders.Commands.Orders.CancelOrder;
using LocMp.Order.Application.Orders.Commands.Orders.CompleteOrder;
using LocMp.Order.Application.Orders.Commands.Orders.ConfirmOrder;
using LocMp.Order.Application.Orders.Commands.Orders.MarkReadyForPickup;
using LocMp.Order.Application.Orders.Commands.Orders.DeleteOrderPhoto;
using LocMp.Order.Application.Orders.Commands.Orders.UploadOrderPhotos;
using LocMp.Order.Application.Orders.Queries.Orders.GetAllOrders;
using LocMp.Order.Application.Orders.Queries.Orders.GetOrderById;
using LocMp.Order.Application.Orders.Queries.Orders.GetOrdersByBuyer;
using LocMp.Order.Application.Orders.Queries.Orders.GetOrdersBySeller;
using LocMp.Order.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocMp.Order.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class OrdersController(ISender sender) : ControllerBase
{
    // ── Queries ────────────────────────────────────────────────────────────

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken ct)
    {
        var isAdmin = HttpContext.IsInRole("Admin");
        var result = await sender.Send(new GetOrderByIdQuery(id, HttpContext.GetUserId(), isAdmin), ct);
        return Ok(result);
    }

    [HttpGet("my-purchases")]
    public async Task<ActionResult<PagedResult<OrderSummaryDto>>> GetMyPurchases(
        [FromQuery] OrderStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new GetOrdersByBuyerQuery(HttpContext.GetUserId(), status, page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("my-sales")]
    [Authorize(Roles = "Seller,Admin")]
    public async Task<ActionResult<PagedResult<OrderSummaryDto>>> GetMySales(
        [FromQuery] OrderStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new GetOrdersBySellerQuery(HttpContext.GetUserId(), status, page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("seller/{sellerId:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<OrderSummaryDto>>> GetBySeller(
        Guid sellerId,
        [FromQuery] OrderStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new GetOrdersBySellerQuery(sellerId, status, page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<OrderSummaryDto>>> GetAll(
        [FromQuery] OrderStatus? status,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] decimal? minAmount,
        [FromQuery] decimal? maxAmount,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new GetAllOrdersQuery(status, from, to, minAmount, maxAmount, page, pageSize), ct);
        return Ok(result);
    }

    // ── Status transitions ─────────────────────────────────────────────────

    [HttpPost("{id:guid}/confirm")]
    [Authorize(Roles = "Seller,Admin")]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken ct)
    {
        await sender.Send(new ConfirmOrderCommand(id, HttpContext.GetUserId()), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/ready")]
    [Authorize(Roles = "Seller,Admin")]
    public async Task<IActionResult> MarkReady(Guid id, CancellationToken ct)
    {
        await sender.Send(new MarkReadyForPickupCommand(id, HttpContext.GetUserId()), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/assign-courier")]
    [Authorize(Roles = "Seller,Admin")]
    public async Task<IActionResult> AssignCourier(Guid id, [FromBody] AssignCourierRequest request,
        CancellationToken ct)
    {
        await sender.Send(
            new AssignCourierCommand(id, request.CourierId, request.CourierName, request.CourierPhone), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, CancellationToken ct)
    {
        await sender.Send(new CompleteOrderCommand(id, HttpContext.GetUserId()), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelOrderRequest request, CancellationToken ct)
    {
        var isAdmin = HttpContext.IsInRole("Admin");
        await sender.Send(new CancelOrderCommand(id, HttpContext.GetUserId(), isAdmin, request.Comment), ct);
        return NoContent();
    }

    // ── Photos ─────────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/photos")]
    public async Task<ActionResult<IReadOnlyList<OrderPhotoDto>>> UploadPhotos(
        Guid id, [FromForm] IFormFileCollection images, CancellationToken ct)
    {
        var isAdmin = HttpContext.IsInRole("Admin");
        var result = await sender.Send(
            new UploadOrderPhotosCommand(id, HttpContext.GetUserId(), isAdmin, images.ToList()), ct);
        return Ok(result);
    }

    [HttpDelete("photos/{photoId:guid}")]
    public async Task<IActionResult> DeletePhoto(Guid photoId, CancellationToken ct)
    {
        var isAdmin = HttpContext.IsInRole("Admin");
        await sender.Send(new DeleteOrderPhotoCommand(photoId, HttpContext.GetUserId(), isAdmin), ct);
        return NoContent();
    }
}