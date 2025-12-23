using FitnessProject.BLL.DTO.Meal;
using FitnessProject.BLL.DTO.Recipe;

namespace FitnessProject.BLL.DTO.DailyDietPlan;

public class MealWithRecipesDto
{
    public MealDetailsDto Meal { get; set; } = null!;
    public ICollection<RecipeDetailsDto> Recipes { get; set; } = new List<RecipeDetailsDto>();
}

public class DailyDietPlanFullDetailsDto
{
    public DailyDietPlanResponseDto Plan { get; set; } = null!;
    public ICollection<MealWithRecipesDto> Meals { get; set; } = new List<MealWithRecipesDto>();
}


