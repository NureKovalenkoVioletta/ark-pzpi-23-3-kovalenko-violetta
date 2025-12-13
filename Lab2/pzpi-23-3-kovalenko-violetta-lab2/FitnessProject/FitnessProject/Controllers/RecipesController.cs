using Microsoft.AspNetCore.Mvc;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.BLL.DTO.Recipe;

namespace FitnessProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecipesController : ControllerBase
{
    private readonly IRecipeService _recipeService;

    public RecipesController(IRecipeService recipeService)
    {
        _recipeService = recipeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RecipeResponseDto>>> GetAll()
    {
        var recipes = await _recipeService.GetAllAsync();
        return Ok(recipes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RecipeResponseDto>> GetById(int id)
    {
        var recipe = await _recipeService.GetByIdAsync(id);
        if (recipe == null)
            return NotFound();

        return Ok(recipe);
    }

    [HttpPost]
    public async Task<ActionResult<RecipeResponseDto>> Create([FromBody] RecipeCreateDto createDto)
    {
        try
        {
            var created = await _recipeService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = created.RecipeId }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<RecipeResponseDto>> Update(int id, [FromBody] RecipeUpdateDto updateDto)
    {
        if (id != updateDto.RecipeId)
            return BadRequest(new { error = "ID mismatch" });

        try
        {
            var updated = await _recipeService.UpdateAsync(updateDto);
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
        var deleted = await _recipeService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}

