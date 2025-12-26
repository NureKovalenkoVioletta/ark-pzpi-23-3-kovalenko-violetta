using Microsoft.AspNetCore.Mvc;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.BLL.DTO.RecipeProduct;
using FitnessProject.Resources;
using Microsoft.Extensions.Localization;

namespace FitnessProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecipeProductsController : ControllerBase
{
    private readonly IRecipeProductService _recipeProductService;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public RecipeProductsController(IRecipeProductService recipeProductService, IStringLocalizer<SharedResources> localizer)
    {
        _recipeProductService = recipeProductService;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] int? recipeId, [FromQuery] int? productId)
    {
        var recipeProducts = await _recipeProductService.GetAllAsync();

        if (recipeId.HasValue && productId.HasValue)
        {
            var recipeProduct = recipeProducts.FirstOrDefault(rp => rp.RecipeId == recipeId.Value && rp.ProductId == productId.Value);
            if (recipeProduct == null)
                return NotFound(new { error = _localizer["Errors.NotFound"] });
            return Ok(recipeProduct);
        }

        return Ok(recipeProducts);
    }

    [HttpGet("details")]
    public async Task<ActionResult<RecipeProductDetailsDto>> GetDetailsById([FromQuery] int recipeId, [FromQuery] int productId)
    {
        var recipeProduct = await _recipeProductService.GetRecipeProductDetailsByIdAsync(recipeId, productId);
        if (recipeProduct == null)
            return NotFound(new { error = _localizer["Errors.NotFound"] });

        return Ok(recipeProduct);
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
            return BadRequest(new { error = _localizer["Errors.BadRequest"], details = ex.Message });
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
            return NotFound(new { error = _localizer["Errors.NotFound"], details = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = _localizer["Errors.BadRequest"], details = ex.Message });
        }
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] int recipeId, [FromQuery] int productId)
    {
        var recipeProducts = await _recipeProductService.GetAllAsync();
        var recipeProduct = recipeProducts.FirstOrDefault(rp => rp.RecipeId == recipeId && rp.ProductId == productId);
        if (recipeProduct == null)
            return NotFound(new { error = _localizer["Errors.NotFound"] });

        var deleted = await _recipeProductService.DeleteAsync(recipeId);
        if (!deleted)
            return NotFound(new { error = _localizer["Errors.NotFound"] });

        return NoContent();
    }
}

