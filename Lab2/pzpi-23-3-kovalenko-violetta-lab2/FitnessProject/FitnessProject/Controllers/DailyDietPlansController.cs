using Microsoft.AspNetCore.Mvc;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.BLL.DTO.DailyDietPlan;

namespace FitnessProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DailyDietPlansController : ControllerBase
{
    private readonly IDailyDietPlanService _dailyDietPlanService;

    public DailyDietPlansController(IDailyDietPlanService dailyDietPlanService)
    {
        _dailyDietPlanService = dailyDietPlanService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DailyDietPlanResponseDto>>> GetAll()
    {
        var plans = await _dailyDietPlanService.GetAllAsync();
        return Ok(plans);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DailyDietPlanResponseDto>> GetById(int id)
    {
        var plan = await _dailyDietPlanService.GetByIdAsync(id);
        if (plan == null)
            return NotFound();

        return Ok(plan);
    }

    [HttpPost]
    public async Task<ActionResult<DailyDietPlanResponseDto>> Create([FromBody] DailyDietPlanCreateDto createDto)
    {
        try
        {
            var created = await _dailyDietPlanService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = created.DailyDietPlanId }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DailyDietPlanResponseDto>> Update(int id, [FromBody] DailyDietPlanUpdateDto updateDto)
    {
        if (id != updateDto.DailyDietPlanId)
            return BadRequest(new { error = "ID mismatch" });

        try
        {
            var updated = await _dailyDietPlanService.UpdateAsync(updateDto);
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
        var deleted = await _dailyDietPlanService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}

