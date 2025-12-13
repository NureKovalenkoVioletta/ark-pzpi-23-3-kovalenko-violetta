using Microsoft.AspNetCore.Mvc;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.BLL.DTO.RecipeProduct;

namespace FitnessProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecipeProductsController : ControllerBase
{
    private readonly IRecipeProductService _recipeProductService;

    public RecipeProductsController(IRecipeProductService recipeProductService)
    {
        _recipeProductService = recipeProductService;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] int? recipeId, [FromQuery] int? productId)
    {
        var recipeProducts = await _recipeProductService.GetAllAsync();

        if (recipeId.HasValue && productId.HasValue)
        {
            var recipeProduct = recipeProducts.FirstOrDefault(rp => rp.RecipeId == recipeId.Value && rp.ProductId == productId.Value);
            if (recipeProduct == null)
                return NotFound();
            return Ok(recipeProduct);
        }

        return Ok(recipeProducts);
    }

    [HttpPost]
    public async Task<ActionResult<RecipeProductResponseDto>> Create([FromBody] RecipeProductCreateDto createDto)
    {
        try
        {
            var created = await _recipeProductService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetAll), new { recipeId = created.RecipeId, productId = created.ProductId }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut]
    public async Task<ActionResult<RecipeProductResponseDto>> Update([FromBody] RecipeProductUpdateDto updateDto)
    {
        try
        {
            var updated = await _recipeProductService.UpdateAsync(updateDto);
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
    public async Task<IActionResult> Delete([FromQuery] int recipeId, [FromQuery] int productId)
    {
        var recipeProducts = await _recipeProductService.GetAllAsync();
        var recipeProduct = recipeProducts.FirstOrDefault(rp => rp.RecipeId == recipeId && rp.ProductId == productId);
        if (recipeProduct == null)
            return NotFound();

        var deleted = await _recipeProductService.DeleteAsync(recipeId);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}

