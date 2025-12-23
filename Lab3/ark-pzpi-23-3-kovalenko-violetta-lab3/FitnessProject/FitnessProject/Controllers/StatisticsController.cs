using Microsoft.AspNetCore.Mvc;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.BLL.DTO.Services;
using FitnessProject.Resources;
using Microsoft.Extensions.Localization;

namespace FitnessProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _statisticsService;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public StatisticsController(IStatisticsService statisticsService, IStringLocalizer<SharedResources> localizer)
    {
        _statisticsService = statisticsService;
        _localizer = localizer;
    }

    [HttpGet("daily/{date}")]
    public async Task<ActionResult<DailyStatisticsDto>> GetDailyStatistics(DateTime date, [FromQuery] int? userId)
    {
        // TODO: заменить на текущего пользователя после внедрения аутентификации
        var uid = userId ?? 0;

        var result = await _statisticsService.GetDailyStatisticsAsync(uid, date);
        return Ok(result);
    }

    [HttpGet("weekly/{startDate}")]
    public async Task<ActionResult<WeeklyStatisticsDto>> GetWeeklyStatistics(DateTime startDate, [FromQuery] int? userId)
    {
        var uid = userId ?? 0;
        var result = await _statisticsService.GetWeeklyStatisticsAsync(uid, startDate);
        return Ok(result);
    }

    [HttpGet("comparison")]
    public async Task<ActionResult<WeekComparisonDto>> CompareWeeks([FromQuery] DateTime startDate, [FromQuery] int? userId)
    {
        var uid = userId ?? 0;
        var result = await _statisticsService.CompareWithPreviousWeek(uid, startDate);
        return Ok(result);
    }
}

