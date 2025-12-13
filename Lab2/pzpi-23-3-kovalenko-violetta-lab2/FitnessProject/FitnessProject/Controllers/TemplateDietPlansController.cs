using Microsoft.AspNetCore.Mvc;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.BLL.DTO.TemplateDietPlan;

namespace FitnessProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TemplateDietPlansController : ControllerBase
{
    private readonly ITemplateDietPlanService _templateDietPlanService;

    public TemplateDietPlansController(ITemplateDietPlanService templateDietPlanService)
    {
        _templateDietPlanService = templateDietPlanService;
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
            return NotFound();

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
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TemplateDietPlanResponseDto>> Update(int id, [FromBody] TemplateDietPlanUpdateDto updateDto)
    {
        if (id != updateDto.TemplateDietPlanId)
            return BadRequest(new { error = "ID mismatch" });

        try
        {
            var updated = await _templateDietPlanService.UpdateAsync(updateDto);
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
        var deleted = await _templateDietPlanService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}

