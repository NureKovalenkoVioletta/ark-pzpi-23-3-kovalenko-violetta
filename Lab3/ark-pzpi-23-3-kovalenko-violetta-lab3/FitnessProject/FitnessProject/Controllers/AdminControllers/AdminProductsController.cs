using FitnessProject.BLL.DTO.Product;
using FitnessProject.BLL.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FitnessProject.Resources;
using Microsoft.Extensions.Localization;

namespace FitnessProject.Controllers;

[ApiController]
[Route("api/admin/products")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public AdminProductsController(IProductService productService, IStringLocalizer<SharedResources> localizer)
    {
        _productService = productService;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetAll()
    {
        var products = await _productService.GetAllAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductResponseDto>> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
            return NotFound(new { error = _localizer["Errors.NotFound"] });
        return Ok(product);
    }

    [HttpGet("{id}/details")]
    public async Task<ActionResult<ProductDetailsDto>> GetDetailsById(int id)
    {
        var product = await _productService.GetProductDetailsByIdAsync(id);
        if (product == null)
            return NotFound(new { error = _localizer["Errors.NotFound"] });
        return Ok(product);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ProductResponseDto>> Create([FromBody] ProductCreateDto createDto)
    {
        try
        {
            var created = await _productService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = created.ProductId }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = _localizer["Errors.BadRequest"], details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<ProductResponseDto>> Update(int id, [FromBody] ProductUpdateDto updateDto)
    {
        if (id != updateDto.ProductId)
            return BadRequest(new { error = _localizer["Errors.IdMismatch"] });

        try
        {
            var updated = await _productService.UpdateAsync(updateDto);
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
        var deleted = await _productService.DeleteAsync(id);
        if (!deleted)
            return NotFound(new { error = _localizer["Errors.NotFound"] });
        return NoContent();
    }
}

