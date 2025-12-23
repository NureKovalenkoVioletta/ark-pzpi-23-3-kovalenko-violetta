using FitnessProject.BLL.DTO.Meal;
using FitnessProject.BLL.DTO.Recipe;

namespace FitnessProject.BLL.DTO.MealRecipe;

public class MealRecipeDetailsDto
{
    public int MealId { get; set; }
    public int RecipeId { get; set; }
    
    public MealResponseDto Meal { get; set; } = null!;
    public RecipeResponseDto Recipe { get; set; } = null!;
}

