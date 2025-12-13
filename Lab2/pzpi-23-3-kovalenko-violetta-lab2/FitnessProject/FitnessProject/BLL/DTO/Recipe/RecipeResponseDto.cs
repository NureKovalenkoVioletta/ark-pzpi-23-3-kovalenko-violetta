namespace FitnessProject.BLL.DTO.Recipe;

public class RecipeResponseDto
{
    public int RecipeId { get; set; }
    public string RecipeName { get; set; } = string.Empty;
    public string RecipeInstructions { get; set; } = string.Empty;
    public decimal RecipeCaloriesPerPortion { get; set; }
    public decimal RecipeFatPerPortion { get; set; }
    public decimal RecipeCarbsPerPortion { get; set; }
    public decimal RecipeProteinPerPortion { get; set; }
    public decimal RecipeProductsGrams { get; set; }
}

