using LocMp.Analytics.Api.Extensions;
using LocMp.Analytics.Application.Analytics.Commands;
using LocMp.Analytics.Application.Analytics.Queries.Seller;
using LocMp.Analytics.Application.DTOs;
using LocMp.Analytics.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocMp.Analytics.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Seller,Admin")]
public sealed class SellerDashboardController(ISender sender) : ControllerBase
{
    /// <summary>Выручка и заказы продавца за период.</summary>
    [HttpGet("sales")]
    public async Task<ActionResult<SellerSalesSummaryDto>> GetSales(
        [FromQuery] PeriodType period = PeriodType.Monthly,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new GetSellerSalesQuery(HttpContext.GetUserId(), period), ct);
        return result is null ? NoContent() : Ok(result);
    }

    /// <summary>Топ товаров продавца за период.</summary>
    [HttpGet("top-products")]
    public async Task<ActionResult<List<TopProductDto>>> GetTopProducts(
        [FromQuery] PeriodType period = PeriodType.Weekly,
        [FromQuery] int top = 10,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetSellerTopProductsQuery(HttpContext.GetUserId(), period, top), ct));

    /// <summary>Динамика рейтинга за последние N дней.</summary>
    [HttpGet("rating-history")]
    public async Task<ActionResult<List<SellerRatingHistoryDto>>> GetRatingHistory(
        [FromQuery] int days = 30,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetSellerRatingHistoryQuery(HttpContext.GetUserId(), days), ct));

    /// <summary>Алерты об истощении остатков.</summary>
    [HttpGet("stock-alerts")]
    public async Task<ActionResult<List<StockAlertDto>>> GetStockAlerts(
        [FromQuery] bool onlyUnacknowledged = false,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetSellerStockAlertsQuery(HttpContext.GetUserId(), onlyUnacknowledged), ct));

    /// <summary>Отметить алерт как прочитанный.</summary>
    [HttpPost("stock-alerts/{id:guid}/acknowledge")]
    public async Task<IActionResult> AcknowledgeAlert(Guid id, CancellationToken ct)
    {
        await sender.Send(new AcknowledgeStockAlertCommand(id, HttpContext.GetUserId()), ct);
        return NoContent();
    }

    /// <summary>Просмотры товаров. Без productId — все товары продавца.</summary>
    [HttpGet("product-views")]
    public async Task<ActionResult<List<ProductViewCounterDto>>> GetProductViews(
        [FromQuery] Guid? productId = null,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetProductViewCountersQuery(HttpContext.GetUserId(), productId), ct));

    /// <summary>Рейтинги продуктов продавца с агрегатом по всем товарам.</summary>
    [HttpGet("product-ratings")]
    public async Task<ActionResult<SellerProductRatingsDto>> GetProductRatings(CancellationToken ct)
        => Ok(await sender.Send(new GetSellerProductRatingsQuery(HttpContext.GetUserId()), ct));
}
