using FitnessProject.BLL.DTO.Recipe;
using FitnessProject.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FitnessProject.Resources;
using Microsoft.Extensions.Localization;

namespace FitnessProject.Controllers;

[ApiController]
[Route("api/admin/recipes")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminRecipesController : ControllerBase
{
    private readonly IRecipeService _recipeService;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public AdminRecipesController(IRecipeService recipeService, IStringLocalizer<SharedResources> localizer)
    {
        _recipeService = recipeService;
        _localizer = localizer;
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
            return NotFound(new { error = _localizer["Errors.NotFound"] });
        return Ok(recipe);
    }

    [HttpGet("{id}/details")]
    public async Task<ActionResult<RecipeDetailsDto>> GetDetailsById(int id)
    {
        var recipe = await _recipeService.GetRecipeDetailsByIdAsync(id);
        if (recipe == null)
            return NotFound(new { error = _localizer["Errors.NotFound"] });
        return Ok(recipe);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<RecipeResponseDto>> Create([FromBody] RecipeCreateDto createDto)
    {
        try
        {
            var created = await _recipeService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = created.RecipeId }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = _localizer["Errors.BadRequest"], details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<RecipeResponseDto>> Update(int id, [FromBody] RecipeUpdateDto updateDto)
    {
        if (id != updateDto.RecipeId)
            return BadRequest(new { error = _localizer["Errors.IdMismatch"] });

        try
        {
            var updated = await _recipeService.UpdateAsync(updateDto);
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
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _recipeService.DeleteAsync(id);
        if (!deleted)
            return NotFound(new { error = _localizer["Errors.NotFound"] });
        return NoContent();
    }
}

