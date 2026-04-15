using LocMp.BuildingBlocks.Application.Common;
using LocMp.Review.Api.Extensions;
using LocMp.Review.Api.Requests;
using LocMp.Review.Application.DTOs;
using LocMp.Review.Application.Reviews.Commands.CreateReview;
using LocMp.Review.Application.Reviews.Commands.DeleteReview;
using LocMp.Review.Application.Reviews.Commands.DeleteReviewPhoto;
using LocMp.Review.Application.Reviews.Commands.DeleteReviewResponse;
using LocMp.Review.Application.Reviews.Commands.ModerateReview;
using LocMp.Review.Application.Reviews.Commands.RespondToReview;
using LocMp.Review.Application.Reviews.Commands.UpdateReview;
using LocMp.Review.Application.Reviews.Commands.UpdateReviewResponse;
using LocMp.Review.Application.Reviews.Commands.UploadReviewPhotos;
using LocMp.Review.Application.Reviews.Queries.GetAllowedReviewsForBuyer;
using LocMp.Review.Application.Reviews.Queries.GetMyReviews;
using LocMp.Review.Application.Reviews.Queries.GetRatingBySubject;
using LocMp.Review.Application.Reviews.Queries.GetReviewById;
using LocMp.Review.Application.Reviews.Queries.GetReviewByOrder;
using LocMp.Review.Application.Reviews.Queries.GetReviewsBySubject;
using LocMp.Review.Application.Reviews.Queries.GetReviewsForModeration;
using LocMp.Review.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocMp.Review.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class ReviewsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<ReviewSummaryDto>>> GetBySubject(
        [FromQuery] ReviewSubjectType subjectType,
        [FromQuery] Guid subjectId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetReviewsBySubjectQuery(subjectType, subjectId, page, pageSize), ct));

    [HttpGet("rating")]
    [AllowAnonymous]
    public async Task<ActionResult<RatingAggregateDto>> GetRating(
        [FromQuery] ReviewSubjectType subjectType,
        [FromQuery] Guid subjectId,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetRatingBySubjectQuery(subjectType, subjectId), ct));

    [HttpGet("my")]
    public async Task<ActionResult<PagedResult<ReviewSummaryDto>>> GetMy(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetMyReviewsQuery(HttpContext.GetUserId(), page, pageSize), ct));

    [HttpGet("allowed")]
    public async Task<ActionResult<PagedResult<PendingReviewSubjectDto>>> GetAllowed(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetAllowedReviewsForBuyerQuery(HttpContext.GetUserId(), page, pageSize), ct));

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<ReviewSummaryDto>>> GetForModeration(
        [FromQuery] bool? isVisible = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetReviewsForModerationQuery(isVisible, page, pageSize), ct));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ReviewDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetReviewByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("by-order/{orderId:guid}")]
    public async Task<ActionResult<ReviewDto>> GetByOrder(Guid orderId, CancellationToken ct)
    {
        var result = await sender.Send(new GetReviewByOrderQuery(orderId), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ReviewDto>> Create([FromBody] CreateReviewRequest request, CancellationToken ct)
    {
        var userId = HttpContext.GetUserId();
        var reviewerName = User.FindFirst("username")?.Value
                           ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                           ?? "Unknown";

        var result = await sender.Send(new CreateReviewCommand(
            request.OrderId, userId, reviewerName,
            request.SubjectType, request.SubjectId,
            request.Rating, request.Comment), ct);

        return CreatedAtAction(nameof(GetByOrder), new { orderId = request.OrderId }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ReviewDto>> Update(
        Guid id, [FromBody] UpdateReviewRequest request, CancellationToken ct)
        => Ok(await sender.Send(new UpdateReviewCommand(id, HttpContext.GetUserId(), request.Rating, request.Comment),
            ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeleteReviewCommand(id, HttpContext.GetUserId(), HttpContext.IsInRole("Admin")), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/moderate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Moderate(
        Guid id, [FromBody] ModerateReviewRequest request, CancellationToken ct)
    {
        await sender.Send(new ModerateReviewCommand(id, request.IsVisible), ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/response")]
    [Authorize(Roles = "Seller")]
    public async Task<ActionResult<ReviewResponseDto>> Respond(
        Guid id, [FromBody] RespondToReviewRequest request, CancellationToken ct)
        => Ok(await sender.Send(new RespondToReviewCommand(id, HttpContext.GetUserId(), request.Comment), ct));

    [HttpPut("{id:guid}/response")]
    [Authorize(Roles = "Seller")]
    public async Task<ActionResult<ReviewResponseDto>> UpdateResponse(
        Guid id, [FromBody] UpdateReviewResponseRequest request, CancellationToken ct)
        => Ok(await sender.Send(new UpdateReviewResponseCommand(id, HttpContext.GetUserId(), request.Comment), ct));

    [HttpDelete("{id:guid}/response")]
    public async Task<IActionResult> DeleteResponse(Guid id, CancellationToken ct)
    {
        await sender.Send(new DeleteReviewResponseCommand(id, HttpContext.GetUserId(), HttpContext.IsInRole("Admin")),
            ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/photos")]
    public async Task<ActionResult<IReadOnlyList<ReviewPhotoDto>>> UploadPhotos(
        Guid id, [FromForm] IFormFileCollection images, CancellationToken ct)
    {
        var result = await sender.Send(
            new UploadReviewPhotosCommand(id, HttpContext.GetUserId(), HttpContext.IsInRole("Admin"), images.ToList()),
            ct);
        return Ok(result);
    }

    [HttpDelete("photos/{photoId:guid}")]
    public async Task<IActionResult> DeletePhoto(Guid photoId, CancellationToken ct)
    {
        await sender.Send(new DeleteReviewPhotoCommand(photoId, HttpContext.GetUserId(), HttpContext.IsInRole("Admin")),
            ct);
        return NoContent();
    }
}