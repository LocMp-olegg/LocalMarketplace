using LocMp.Order.Api.Extensions;
using LocMp.Order.Application.DTOs;
using LocMp.Order.Application.Orders.Commands.Orders.MarkOrderDelivered;
using LocMp.Order.Application.Orders.Commands.Orders.MarkOrderPickedUp;
using LocMp.Order.Application.Orders.Queries.GetAvailableOrdersForCourier;
using LocMp.Order.Application.Orders.Queries.GetOrdersAssignedToCourier;
using LocMp.Order.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocMp.Order.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Courier")]
public sealed class CourierController(ISender sender) : ControllerBase
{
    [HttpGet("orders/available")]
    public async Task<ActionResult<IReadOnlyList<OrderSummaryDto>>> GetAvailable(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double radiusKm = 5,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new GetAvailableOrdersForCourierQuery(latitude, longitude, radiusKm), ct);
        return Ok(result);
    }

    [HttpGet("orders")]
    public async Task<ActionResult<IReadOnlyList<OrderSummaryDto>>> GetMyDeliveries(
        [FromQuery] OrderStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new GetOrdersAssignedToCourierQuery(HttpContext.GetUserId(), status, page, pageSize), ct);
        return Ok(result);
    }

    [HttpPost("orders/{id:guid}/pickup")]
    public async Task<IActionResult> PickUp(Guid id, CancellationToken ct)
    {
        await sender.Send(new MarkOrderPickedUpCommand(id, HttpContext.GetUserId()), ct);
        return NoContent();
    }

    [HttpPost("orders/{id:guid}/deliver")]
    public async Task<IActionResult> Deliver(Guid id, CancellationToken ct)
    {
        await sender.Send(new MarkOrderDeliveredCommand(id, HttpContext.GetUserId()), ct);
        return NoContent();
    }
}