using Microsoft.AspNetCore.Mvc;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.BLL.DTO.Recommendation;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Enums;
using FitnessProject.Resources;
using Microsoft.Extensions.Localization;

namespace FitnessProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _recommendationService;
    private readonly IRecommendationRepository _recommendationRepository;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public RecommendationsController(
        IRecommendationService recommendationService,
        IRecommendationRepository recommendationRepository,
        IStringLocalizer<SharedResources> localizer)
    {
        _recommendationService = recommendationService;
        _recommendationRepository = recommendationRepository;
        _localizer = localizer;
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
            return NotFound(new { error = _localizer["Errors.NotFound"] });

        return Ok(recommendation);
    }

    [HttpGet("{id}/details")]
    public async Task<ActionResult<RecommendationDetailsDto>> GetDetailsById(int id)
    {
        var recommendation = await _recommendationService.GetRecommendationDetailsByIdAsync(id);
        if (recommendation == null)
            return NotFound(new { error = _localizer["Errors.NotFound"] });

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
            return BadRequest(new { error = _localizer["Errors.BadRequest"], details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<RecommendationResponseDto>> Update(int id, [FromBody] RecommendationUpdateDto updateDto)
    {
        if (id != updateDto.RecommendationId)
            return BadRequest(new { error = _localizer["Errors.IdMismatch"] });

        try
        {
            var updated = await _recommendationService.UpdateAsync(updateDto);
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
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _recommendationService.DeleteAsync(id);
        if (!deleted)
            return NotFound(new { error = _localizer["Errors.NotFound"] });

        return NoContent();
    }

    [HttpGet("corrections")]
    public async Task<ActionResult<IEnumerable<RecommendationResponseDto>>> GetCorrectionRecommendations([FromQuery] int? userId)
    {
        var corrections = await _recommendationRepository.FindAsync(r =>
            r.RecommendationType == RecommendationType.DietCorrection &&
            r.RecommendationStatus == RecommendationStatus.New &&
            (!userId.HasValue || r.Meal != null && r.Meal.DailyDietPlan.UserId == userId.Value));

        var response = corrections.Select(r => new RecommendationResponseDto
        {
            RecommendationId = r.RecommendationId,
            MealInstanceId = r.MealInstanceId,
            RecommendationCreatedAt = r.RecommendationCreatedAt,
            RecommendationType = r.RecommendationType,
            RecommendationPayload = r.RecommendationPayload,
            RecommendationStatus = r.RecommendationStatus
        }).ToList();

        return Ok(response);
    }
}

