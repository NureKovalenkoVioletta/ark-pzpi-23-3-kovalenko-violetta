using Microsoft.AspNetCore.Mvc;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.BLL.DTO.DailyDietPlan;
using FitnessProject.BLL.DTO.Meal;
using FitnessProject.BLL.DTO.Recipe;
using FitnessProject.BLL.DTO.Recommendation;
using Microsoft.AspNetCore.Authorization;
using FitnessProject.Resources;
using Microsoft.Extensions.Localization;

namespace FitnessProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DailyDietPlansController : ControllerBase
{
    private readonly IDailyDietPlanService _dailyDietPlanService;
    private readonly IMealPlanGeneratorService _mealPlanGeneratorService;
    private readonly IMealService _mealService;
    private readonly IRecipeService _recipeService;
    private readonly IDietCorrectionService _dietCorrectionService;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public DailyDietPlansController(
        IDailyDietPlanService dailyDietPlanService,
        IMealPlanGeneratorService mealPlanGeneratorService,
        IMealService mealService,
        IRecipeService recipeService,
        IDietCorrectionService dietCorrectionService,
        IStringLocalizer<SharedResources> localizer)
    {
        _dailyDietPlanService = dailyDietPlanService;
        _mealPlanGeneratorService = mealPlanGeneratorService;
        _mealService = mealService;
        _recipeService = recipeService;
        _dietCorrectionService = dietCorrectionService;
        _localizer = localizer;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DailyDietPlanResponseDto>>> GetAll([FromQuery] int? userId = null)
    {
        IEnumerable<DailyDietPlanResponseDto> plans;
        
        if (userId.HasValue)
        {
            var allPlans = await _dailyDietPlanService.GetAllAsync();
            plans = allPlans.Where(p => p.UserId == userId.Value)
                           .OrderByDescending(p => p.DailyPlanCreatedAt)
                           .ThenByDescending(p => p.DailyDietPlanId);
        }
        else
        {
            plans = await _dailyDietPlanService.GetAllAsync();
        }
        
        return Ok(plans);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DailyDietPlanResponseDto>> GetById(int id)
    {
        var plan = await _dailyDietPlanService.GetByIdAsync(id);
        if (plan == null)
            return NotFound(new { error = _localizer["Errors.NotFound"] });

        return Ok(plan);
    }

    [HttpGet("{id}/details")]
    public async Task<ActionResult<DailyDietPlanDetailsDto>> GetDetailsById(int id)
    {
        var plan = await _dailyDietPlanService.GetDailyDietPlanDetailsByIdAsync(id);
        if (plan == null)
            return NotFound(new { error = _localizer["Errors.NotFound"] });

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
            return BadRequest(new { error = _localizer["Errors.BadRequest"], details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<DailyDietPlanResponseDto>> Update(int id, [FromBody] DailyDietPlanUpdateDto updateDto)
    {
        if (id != updateDto.DailyDietPlanId)
            return BadRequest(new { error = _localizer["Errors.IdMismatch"] });

        try
        {
            var updated = await _dailyDietPlanService.UpdateAsync(updateDto);
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

    [HttpPost("generate")]
    public async Task<ActionResult<DailyDietPlanFullDetailsDto>> GeneratePlan([FromBody] GenerateDietPlanDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var date = dto.Date ?? DateTime.Today;

        try
        {
            var plan = await _mealPlanGeneratorService.GenerateMealPlanAsync(
                dto.UserId,
                date,
                dto.TemplateDietPlanId);

            var planResponse = await _dailyDietPlanService.GetByIdAsync(plan.DailyDietPlanId);
            if (planResponse == null)
            {
                return BadRequest(new { error = _localizer["Errors.GeneratedPlanLoadFailed"] });
            }

            var allMeals = await _mealService.GetAllAsync();
            var mealsForPlan = allMeals.Where(m => m.DailyDietPlanId == plan.DailyDietPlanId).ToList();

            var resultMeals = new List<MealWithRecipesDto>();

            foreach (var meal in mealsForPlan)
            {
                var mealDetails = await _mealService.GetMealDetailsByIdAsync(meal.MealId);
                if (mealDetails == null)
                {
                    continue;
                }

                var recipes = new List<RecipeDetailsDto>();

                foreach (var mealRecipe in mealDetails.MealRecipes)
                {
                    var recipeDetails = await _recipeService.GetRecipeDetailsByIdAsync(mealRecipe.RecipeId);
                    if (recipeDetails != null)
                    {
                        recipes.Add(recipeDetails);
                    }
                }

                resultMeals.Add(new MealWithRecipesDto
                {
                    Meal = mealDetails,
                    Recipes = recipes
                });
            }

            var fullResult = new DailyDietPlanFullDetailsDto
            {
                Plan = planResponse,
                Meals = resultMeals
            };

            return CreatedAtAction(nameof(GetById), new { id = plan.DailyDietPlanId }, fullResult);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = _localizer["Errors.BadRequest"], details = ex.Message });
        }
    }

    [HttpGet("{id}/meals")]
    public async Task<ActionResult<DailyDietPlanMealsDto>> GetMeals(int id)
    {
        var plan = await _dailyDietPlanService.GetByIdAsync(id);
        if (plan == null)
        {
            return NotFound(new { error = _localizer["Errors.NotFound"] });
        }

        var allMeals = await _mealService.GetAllAsync();
        var mealsForPlan = allMeals.Where(m => m.DailyDietPlanId == id).ToList();

        var detailedMeals = new List<MealDetailsDto>();
        foreach (var meal in mealsForPlan)
        {
            var details = await _mealService.GetMealDetailsByIdAsync(meal.MealId);
            if (details != null)
            {
                detailedMeals.Add(details);
            }
        }

        var result = new DailyDietPlanMealsDto
        {
            DailyDietPlanId = plan.DailyDietPlanId,
            DailyDietPlanName = plan.DailyDietPlanName,
            DailyPlanCreatedAt = plan.DailyPlanCreatedAt,
            Meals = detailedMeals
        };

        return Ok(result);
    }

    [HttpPost("{id}/regenerate")]
    public async Task<ActionResult<DailyDietPlanResponseDto>> RegeneratePlan(int id)
    {
        var existingPlan = await _dailyDietPlanService.GetByIdAsync(id);
        if (existingPlan == null)
        {
            return NotFound();
        }

        var date = existingPlan.DailyPlanCreatedAt.Date;

        try
        {
            var newPlan = await _mealPlanGeneratorService.GenerateMealPlanAsync(
                existingPlan.UserId,
                date,
                existingPlan.TemplateDietPlanId);

            var response = await _dailyDietPlanService.GetByIdAsync(newPlan.DailyDietPlanId);
            return CreatedAtAction(nameof(GetById), new { id = newPlan.DailyDietPlanId }, response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = _localizer["Errors.BadRequest"], details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _dailyDietPlanService.DeleteAsync(id);
        if (!deleted)
            return NotFound(new { error = _localizer["Errors.NotFound"] });

        return NoContent();
    }

    [HttpPost("{id}/check-corrections")]
    public async Task<ActionResult<IEnumerable<RecommendationResponseDto>>> CheckCorrections(int id)
    {
        var plan = await _dailyDietPlanService.GetByIdAsync(id);
        if (plan == null)
        {
            return NotFound(new { error = _localizer["Errors.NotFound"] });
        }

        try
        {
            var recs = await _dietCorrectionService.CheckAndSuggestCorrectionsAsync(plan.UserId, id);
            var response = recs.Select(r => new RecommendationResponseDto
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
        catch (Exception ex)
        {
            return BadRequest(new { error = _localizer["Errors.BadRequest"], details = ex.Message });
        }
    }

    [HttpPost("{id}/apply-correction")] 
    public async Task<ActionResult<DailyDietPlanResponseDto>> ApplyCorrection(int id, [FromBody] ApplyCorrectionDto dto)
    {
        if (dto == null)
        {
            return BadRequest(new { error = _localizer["Errors.RecommendationIdRequired"] });
        }

        try
        {
            var updated = await _dietCorrectionService.ApplyCorrectionAsync(id, dto.RecommendationId);
            var response = await _dailyDietPlanService.GetByIdAsync(updated.DailyDietPlanId);
            if (response == null)
            {
                return BadRequest(new { error = _localizer["Errors.UpdatedPlanLoadFailed"] });
            }

            return Ok(response);
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
}

