using Microsoft.AspNetCore.Mvc;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.BLL.DTO.Recommendation;

namespace FitnessProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;

    public RecommendationsController(IRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RecommendationResponseDto>>> GetAll()
    {
        var recommendations = await _recommendationService.GetAllAsync();
        return Ok(recommendations);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RecommendationResponseDto>> GetById(int id)
    {
        var recommendation = await _recommendationService.GetByIdAsync(id);
        if (recommendation == null)
            return NotFound();

        return Ok(recommendation);
    }

    [HttpPost]
    public async Task<ActionResult<RecommendationResponseDto>> Create([FromBody] RecommendationCreateDto createDto)
    {
        try
        {
            var created = await _recommendationService.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = created.RecommendationId }, created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<RecommendationResponseDto>> Update(int id, [FromBody] RecommendationUpdateDto updateDto)
    {
        if (id != updateDto.RecommendationId)
            return BadRequest(new { error = "ID mismatch" });

        try
        {
            var updated = await _recommendationService.UpdateAsync(updateDto);
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
        var deleted = await _recommendationService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}

