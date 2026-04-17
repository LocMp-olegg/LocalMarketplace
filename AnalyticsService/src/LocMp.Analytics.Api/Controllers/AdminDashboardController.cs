using LocMp.Analytics.Application.Analytics.Queries.Admin;
using LocMp.Analytics.Application.DTOs;
using LocMp.Analytics.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocMp.Analytics.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public sealed class AdminDashboardController(ISender sender) : ControllerBase
{
    /// <summary>Сводка по платформе за диапазон дат.</summary>
    [HttpGet("platform")]
    public async Task<ActionResult<List<PlatformDailySummaryDto>>> GetPlatformSummary(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetPlatformSummaryQuery(from, to), ct));

    /// <summary>Рейтинг продавцов по выручке за период.</summary>
    [HttpGet("sellers/leaderboard")]
    public async Task<ActionResult<List<SellerLeaderboardDto>>> GetLeaderboard(
        [FromQuery] PeriodType period = PeriodType.Monthly,
        [FromQuery] int top = 10,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetSellerLeaderboardQuery(period, top), ct));

    /// <summary>Статистика споров. Без параметров — все данные.</summary>
    [HttpGet("disputes")]
    public async Task<ActionResult<List<DisputeSummaryDto>>> GetDisputeSummary(
        [FromQuery] DateOnly? from = null,
        [FromQuery] DateOnly? to   = null,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetDisputeSummaryQuery(from, to), ct));

    /// <summary>Активность по жилым комплексам за период.</summary>
    [HttpGet("geography")]
    public async Task<ActionResult<List<GeographicActivityDto>>> GetGeographicActivity(
        [FromQuery] PeriodType period = PeriodType.Monthly,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetGeographicActivityQuery(period), ct));

    /// <summary>Рост пользователей по дням.</summary>
    [HttpGet("users/growth")]
    public async Task<ActionResult<List<UserGrowthDto>>> GetUsersGrowth(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to,
        CancellationToken ct = default)
        => Ok(await sender.Send(new GetUsersGrowthQuery(from, to), ct));
}
