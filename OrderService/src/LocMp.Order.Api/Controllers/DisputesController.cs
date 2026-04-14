using LocMp.BuildingBlocks.Application.Common;
using LocMp.Order.Api.Extensions;
using LocMp.Order.Api.Requests;
using LocMp.Order.Application.DTOs;
using LocMp.Order.Application.Orders.Commands.Disputes.DeleteDisputePhoto;
using LocMp.Order.Application.Orders.Commands.Disputes.OpenDispute;
using LocMp.Order.Application.Orders.Commands.Disputes.ResolveDispute;
using LocMp.Order.Application.Orders.Commands.Disputes.UploadDisputePhotos;
using LocMp.Order.Application.Orders.Queries.Disputes.GetAllDisputes;
using LocMp.Order.Application.Orders.Queries.Disputes.GetDisputeById;
using LocMp.Order.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocMp.Order.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class DisputesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<DisputeSummaryDto>>> GetAll(
        [FromQuery] DisputeStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new GetAllDisputesQuery(status, page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("{disputeId:guid}")]
    public async Task<ActionResult<DisputeDto>> GetById(Guid disputeId, CancellationToken ct)
    {
        var isAdmin = HttpContext.IsInRole("Admin");
        var result = await sender.Send(
            new GetDisputeByIdQuery(disputeId, HttpContext.GetUserId(), isAdmin), ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/dispute")]
    public async Task<IActionResult> OpenDispute(Guid id, [FromBody] OpenDisputeRequest request,
        CancellationToken ct)
    {
        await sender.Send(new OpenDisputeCommand(id, HttpContext.GetUserId(), request.Reason), ct);
        return NoContent();
    }

    [HttpPost("{disputeId:guid}/resolve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Resolve(
        Guid disputeId, [FromBody] ResolveDisputeRequest request, CancellationToken ct)
    {
        await sender.Send(
            new ResolveDisputeCommand(disputeId, HttpContext.GetUserId(), request.Outcome, request.Resolution), ct);
        return NoContent();
    }

    [HttpPost("{disputeId:guid}/photos")]
    public async Task<ActionResult<IReadOnlyList<DisputePhotoDto>>> UploadPhotos(
        Guid disputeId, [FromForm] IFormFileCollection images, CancellationToken ct)
    {
        var isAdmin = HttpContext.IsInRole("Admin");
        var result = await sender.Send(
            new UploadDisputePhotosCommand(disputeId, HttpContext.GetUserId(), isAdmin, images.ToList()), ct);
        return Ok(result);
    }

    [HttpDelete("photos/{photoId:guid}")]
    public async Task<IActionResult> DeletePhoto(Guid photoId, CancellationToken ct)
    {
        var isAdmin = HttpContext.IsInRole("Admin");
        await sender.Send(new DeleteDisputePhotoCommand(photoId, HttpContext.GetUserId(), isAdmin), ct);
        return NoContent();
    }
}