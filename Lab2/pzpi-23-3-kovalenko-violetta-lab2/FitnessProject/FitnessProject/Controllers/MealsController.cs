using Microsoft.AspNetCore.Mvc;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.BLL.DTO.Meal;

namespace FitnessProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MealsController : ControllerBase
{
    private readonly IMealService _mealService;

    public MealsController(IMealService mealService)
    {
        _mealService = mealService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MealResponseDto>>> GetAll()
    {
        var meals = await _mealService.GetAllAsync();
        return Ok(meals);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MealResponseDto>> GetById(int id)
    {
        var meal = await _mealService.GetByIdAsync(id);
        if (meal == null)
            return NotFound();

        return Ok(meal);
    }

    [HttpPost]
    public async Task<ActionResult<MealResponseDto>> Create([FromBody] MealCreateDto createDto)
    {
        try
        {
            var created = await _mealService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = created.MealId }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<MealResponseDto>> Update(int id, [FromBody] MealUpdateDto updateDto)
    {
        if (id != updateDto.MealId)
            return BadRequest(new { error = "ID mismatch" });

        try
        {
            var updated = await _mealService.UpdateAsync(updateDto);
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
        var deleted = await _mealService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}

