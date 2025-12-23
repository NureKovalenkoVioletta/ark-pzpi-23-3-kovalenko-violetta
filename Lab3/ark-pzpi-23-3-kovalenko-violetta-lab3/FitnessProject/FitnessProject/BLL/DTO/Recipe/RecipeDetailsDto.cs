using FitnessProject.BLL.DTO.MealRecipe;
using FitnessProject.BLL.DTO.RecipeProduct;

namespace FitnessProject.BLL.DTO.Recipe;

public class RecipeDetailsDto
{
    public int RecipeId { get; set; }
    public string RecipeName { get; set; } = string.Empty;
    public string RecipeInstructions { get; set; } = string.Empty;
    public decimal RecipeCaloriesPerPortion { get; set; }
    public decimal RecipeFatPerPortion { get; set; }
    public decimal RecipeCarbsPerPortion { get; set; }
    public decimal RecipeProteinPerPortion { get; set; }
    public decimal RecipeProductsGrams { get; set; }
    
    public ICollection<MealRecipeResponseDto> MealRecipes { get; set; } = new List<MealRecipeResponseDto>();
    public ICollection<RecipeProductDetailsDto> RecipeProducts { get; set; } = new List<RecipeProductDetailsDto>();
}

