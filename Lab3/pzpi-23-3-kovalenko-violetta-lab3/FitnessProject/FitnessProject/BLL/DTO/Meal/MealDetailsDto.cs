using FitnessProject.BLL.DTO.DailyDietPlan;
using FitnessProject.BLL.DTO.MealRecipe;
using FitnessProject.BLL.DTO.Recommendation;
using FitnessProject.Enums;

namespace FitnessProject.BLL.DTO.Meal;

public class MealDetailsDto
{
    public int MealId { get; set; }
    public int DailyDietPlanId { get; set; }
    public MealTime MealTime { get; set; }
    public int MealOrder { get; set; }
    public decimal MealTargetCalories { get; set; }
    public decimal MealTargetFat { get; set; }
    public decimal MealTargetCarbs { get; set; }
    public decimal MealTargetProtein { get; set; }
    
    public DailyDietPlanResponseDto DailyDietPlan { get; set; } = null!;
    public ICollection<MealRecipeResponseDto> MealRecipes { get; set; } = new List<MealRecipeResponseDto>();
    public ICollection<RecommendationResponseDto> Recommendations { get; set; } = new List<RecommendationResponseDto>();
}

