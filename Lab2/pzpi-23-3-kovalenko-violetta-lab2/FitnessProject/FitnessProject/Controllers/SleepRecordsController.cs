using Microsoft.AspNetCore.Mvc;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.BLL.DTO.SleepRecord;

namespace FitnessProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SleepRecordsController : ControllerBase
{
    private readonly ISleepRecordService _sleepRecordService;

    public SleepRecordsController(ISleepRecordService sleepRecordService)
    {
        _sleepRecordService = sleepRecordService;
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
            return NotFound();

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
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SleepRecordResponseDto>> Update(int id, [FromBody] SleepRecordUpdateDto updateDto)
    {
        if (id != updateDto.SleepId)
            return BadRequest(new { error = "ID mismatch" });

        try
        {
            var updated = await _sleepRecordService.UpdateAsync(updateDto);
            return Ok(updated);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _sleepRecordService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}

