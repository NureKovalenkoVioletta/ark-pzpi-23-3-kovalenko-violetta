using Microsoft.AspNetCore.Mvc;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.BLL.DTO.TemplateDietPlan;
using FitnessProject.Resources;
using Microsoft.Extensions.Localization;

namespace FitnessProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TemplateDietPlansController : ControllerBase
{
    private readonly ITemplateDietPlanService _templateDietPlanService;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public TemplateDietPlansController(ITemplateDietPlanService templateDietPlanService, IStringLocalizer<SharedResources> localizer)
    {
        _templateDietPlanService = templateDietPlanService;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TemplateDietPlanResponseDto>>> GetAll()
    {
        var plans = await _templateDietPlanService.GetAllAsync();
        return Ok(plans);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TemplateDietPlanResponseDto>> GetById(int id)
    {
        var plan = await _templateDietPlanService.GetByIdAsync(id);
        if (plan == null)
            return NotFound(new { error = _localizer["Errors.NotFound"] });

        return Ok(plan);
    }

    [HttpGet("{id}/details")]
    public async Task<ActionResult<TemplateDietPlanDetailsDto>> GetDetailsById(int id)
    {
        var plan = await _templateDietPlanService.GetTemplateDietPlanDetailsByIdAsync(id);
        if (plan == null)
            return NotFound(new { error = _localizer["Errors.NotFound"] });

        return Ok(plan);
    }

    [HttpPost]
    public async Task<ActionResult<TemplateDietPlanResponseDto>> Create([FromBody] TemplateDietPlanCreateDto createDto)
    {
        try
        {
            var created = await _templateDietPlanService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = created.TemplateDietPlanId }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = _localizer["Errors.BadRequest"], details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TemplateDietPlanResponseDto>> Update(int id, [FromBody] TemplateDietPlanUpdateDto updateDto)
    {
        if (id != updateDto.TemplateDietPlanId)
            return BadRequest(new { error = _localizer["Errors.IdMismatch"] });

        try
        {
            var updated = await _templateDietPlanService.UpdateAsync(updateDto);
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
        var deleted = await _templateDietPlanService.DeleteAsync(id);
        if (!deleted)
            return NotFound(new { error = _localizer["Errors.NotFound"] });

        return NoContent();
    }
}

