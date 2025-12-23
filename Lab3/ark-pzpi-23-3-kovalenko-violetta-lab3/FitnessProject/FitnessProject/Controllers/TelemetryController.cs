using Microsoft.AspNetCore.Mvc;
using FitnessProject.BLL.DTO.Telemetry;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.Resources;
using Microsoft.Extensions.Localization;

namespace FitnessProject.Controllers;

[ApiController]
[Route("api/telemetry")]
public class TelemetryController : ControllerBase
{
    private readonly ITelemetryProcessingService _telemetryProcessingService;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public TelemetryController(ITelemetryProcessingService telemetryProcessingService, IStringLocalizer<SharedResources> localizer)
    {
        _telemetryProcessingService = telemetryProcessingService;
        _localizer = localizer;
    }

    [HttpPost("receive")]
    public async Task<IActionResult> ReceiveTelemetry([FromBody] TelemetryReceiveDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _telemetryProcessingService.ProcessTelemetryAsync(dto);
            return Ok(new { message = _localizer["Telemetry.ReceiveSuccess"] });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = _localizer["Errors.BadRequest"], details = ex.Message });
        }
    }

    [HttpPost("receive/batch")]
    public async Task<IActionResult> ReceiveTelemetryBatch([FromBody] TelemetryReceiveBatchDto batchDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await _telemetryProcessingService.ProcessBatchAsync(batchDto);
            return Ok(new { message = string.Format(_localizer["Telemetry.BatchSuccess"], batchDto.Items.Count) });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = _localizer["Errors.BadRequest"], details = ex.Message });
        }
    }
}

