using Microsoft.AspNetCore.Mvc;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.BLL.DTO.TelemetrySample;
using FitnessProject.Resources;
using Microsoft.Extensions.Localization;

namespace FitnessProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TelemetrySamplesController : ControllerBase
{
    private readonly ITelemetrySampleService _telemetrySampleService;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public TelemetrySamplesController(ITelemetrySampleService telemetrySampleService, IStringLocalizer<SharedResources> localizer)
    {
        _telemetrySampleService = telemetrySampleService;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TelemetrySampleResponseDto>>> GetAll()
    {
        var samples = await _telemetrySampleService.GetAllAsync();
        return Ok(samples);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TelemetrySampleResponseDto>> GetById(int id)
    {
        var sample = await _telemetrySampleService.GetByIdAsync(id);
        if (sample == null)
            return NotFound(new { error = _localizer["Errors.NotFound"] });

        return Ok(sample);
    }

    [HttpGet("{id}/details")]
    public async Task<ActionResult<TelemetrySampleDetailsDto>> GetDetailsById(int id)
    {
        var sample = await _telemetrySampleService.GetTelemetrySampleDetailsByIdAsync(id);
        if (sample == null)
            return NotFound(new { error = _localizer["Errors.NotFound"] });

        return Ok(sample);
    }

    [HttpPost]
    public async Task<ActionResult<TelemetrySampleResponseDto>> Create([FromBody] TelemetrySampleCreateDto createDto)
    {
        try
        {
            var created = await _telemetrySampleService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = created.SampleId }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = _localizer["Errors.BadRequest"], details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TelemetrySampleResponseDto>> Update(int id, [FromBody] TelemetrySampleUpdateDto updateDto)
    {
        if (id != updateDto.SampleId)
            return BadRequest(new { error = _localizer["Errors.IdMismatch"] });

        try
        {
            var updated = await _telemetrySampleService.UpdateAsync(updateDto);
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
        var deleted = await _telemetrySampleService.DeleteAsync(id);
        if (!deleted)
            return NotFound(new { error = _localizer["Errors.NotFound"] });

        return NoContent();
    }
}

