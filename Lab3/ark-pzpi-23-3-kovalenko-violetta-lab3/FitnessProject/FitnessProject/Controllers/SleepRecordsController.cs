using Microsoft.AspNetCore.Mvc;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.BLL.DTO.SleepRecord;
using FitnessProject.Resources;
using Microsoft.Extensions.Localization;

namespace FitnessProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SleepRecordsController : ControllerBase
{
    private readonly ISleepRecordService _sleepRecordService;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public SleepRecordsController(ISleepRecordService sleepRecordService, IStringLocalizer<SharedResources> localizer)
    {
        _sleepRecordService = sleepRecordService;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SleepRecordResponseDto>>> GetAll()
    {
        var records = await _sleepRecordService.GetAllAsync();
        return Ok(records);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SleepRecordResponseDto>> GetById(int id)
    {
        var record = await _sleepRecordService.GetByIdAsync(id);
        if (record == null)
            return NotFound(new { error = _localizer["Errors.NotFound"] });

        return Ok(record);
    }

    [HttpGet("{id}/details")]
    public async Task<ActionResult<SleepRecordDetailsDto>> GetDetailsById(int id)
    {
        var record = await _sleepRecordService.GetSleepRecordDetailsByIdAsync(id);
        if (record == null)
            return NotFound(new { error = _localizer["Errors.NotFound"] });

        return Ok(record);
    }

    [HttpPost]
    public async Task<ActionResult<SleepRecordResponseDto>> Create([FromBody] SleepRecordCreateDto createDto)
    {
        try
        {
            var created = await _sleepRecordService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = created.SleepId }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = _localizer["Errors.BadRequest"], details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SleepRecordResponseDto>> Update(int id, [FromBody] SleepRecordUpdateDto updateDto)
    {
        if (id != updateDto.SleepId)
            return BadRequest(new { error = _localizer["Errors.IdMismatch"] });

        try
        {
            var updated = await _sleepRecordService.UpdateAsync(updateDto);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = _localizer["Errors.NotFound"], details = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = _localizer["Errors.BadRequest"], details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _sleepRecordService.DeleteAsync(id);
        if (!deleted)
            return NotFound(new { error = _localizer["Errors.NotFound"] });

        return NoContent();
    }
}

