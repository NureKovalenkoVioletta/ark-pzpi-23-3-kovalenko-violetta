using Microsoft.AspNetCore.Mvc;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.BLL.DTO.MealRecipe;

namespace FitnessProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MealRecipesController : ControllerBase
{
    private readonly IMealRecipeService _mealRecipeService;

    public MealRecipesController(IMealRecipeService mealRecipeService)
    {
        _mealRecipeService = mealRecipeService;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] int? mealId, [FromQuery] int? recipeId)
    {
        var mealRecipes = await _mealRecipeService.GetAllAsync();

        if (mealId.HasValue && recipeId.HasValue)
        {
            var mealRecipe = mealRecipes.FirstOrDefault(mr => mr.MealId == mealId.Value && mr.RecipeId == recipeId.Value);
            if (mealRecipe == null)
                return NotFound();
            return Ok(mealRecipe);
        }

        return Ok(mealRecipes);
    }

    [HttpPost]
    public async Task<ActionResult<MealRecipeResponseDto>> Create([FromBody] MealRecipeCreateDto createDto)
    {
        try
        {
            var created = await _mealRecipeService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetAll), new { mealId = created.MealId, recipeId = created.RecipeId }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut]
    public async Task<ActionResult<MealRecipeResponseDto>> Update([FromBody] MealRecipeUpdateDto updateDto)
    {
        try
        {
            var updated = await _mealRecipeService.UpdateAsync(updateDto);
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

    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] int mealId, [FromQuery] int recipeId)
    {
        var mealRecipes = await _mealRecipeService.GetAllAsync();
        var mealRecipe = mealRecipes.FirstOrDefault(mr => mr.MealId == mealId && mr.RecipeId == recipeId);
        if (mealRecipe == null)
            return NotFound();

        var deleted = await _mealRecipeService.DeleteAsync(mealId);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}

