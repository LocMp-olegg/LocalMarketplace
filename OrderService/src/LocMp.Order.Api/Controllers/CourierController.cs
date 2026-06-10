using LocMp.BuildingBlocks.Application.Common;
using LocMp.BuildingBlocks.Infrastructure.Extensions;
using LocMp.Order.Api.Requests;
using LocMp.Order.Application.DTOs;
using LocMp.Order.Application.Orders.Commands.Orders.ApplyCourier;
using LocMp.Order.Application.Orders.Commands.Orders.MarkOrderDelivered;
using LocMp.Order.Application.Orders.Commands.Orders.MarkOrderPickedUp;
using LocMp.Order.Application.Orders.Commands.Orders.WithdrawCourierApplication;
using LocMp.Order.Application.Orders.Queries.Orders.GetAvailableOrdersForCourier;
using LocMp.Order.Application.Orders.Queries.Orders.GetMyCourierApplications;
using LocMp.Order.Application.Orders.Queries.Orders.GetOrdersAssignedToCourier;
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
    public async Task<ActionResult<PagedResult<OrderSummaryDto>>> GetAvailable(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] double radiusKm = 5,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new GetAvailableOrdersForCourierQuery(latitude, longitude, radiusKm, page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("orders")]
    public async Task<ActionResult<PagedResult<OrderSummaryDto>>> GetMyDeliveries(
        [FromQuery] OrderStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new GetOrdersAssignedToCourierQuery(HttpContext.GetUserId(), status, page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("applications")]
    public async Task<ActionResult<PagedResult<CourierApplicationDto>>> GetMyApplications(
        [FromQuery] CourierApplicationStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new GetMyCourierApplicationsQuery(HttpContext.GetUserId(), status, page, pageSize), ct);
        return Ok(result);
    }

    [HttpPost("orders/{id:guid}/apply")]
    public async Task<ActionResult<CourierApplicationDto>> Apply(
        Guid id,
        [FromBody] ApplyCourierRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            new ApplyCourierCommand(HttpContext.GetUserId(), id, request.CourierName, request.CourierPhone,
                request.Latitude, request.Longitude), ct);
        return Ok(result);
    }

    [HttpDelete("applications/{applicationId:guid}")]
    public async Task<IActionResult> Withdraw(Guid applicationId, CancellationToken ct)
    {
        await sender.Send(new WithdrawCourierApplicationCommand(HttpContext.GetUserId(), applicationId), ct);
        return NoContent();
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
