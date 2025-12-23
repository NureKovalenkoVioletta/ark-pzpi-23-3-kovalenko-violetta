namespace FitnessProject.BLL.DTO.Recipe;

public class RecipeUpdateDto
{
    public int RecipeId { get; set; }
    public string? RecipeName { get; set; }
    public string? RecipeInstructions { get; set; }
    public decimal? RecipeCaloriesPerPortion { get; set; }
    public decimal? RecipeFatPerPortion { get; set; }
    public decimal? RecipeCarbsPerPortion { get; set; }
    public decimal? RecipeProteinPerPortion { get; set; }
    public decimal? RecipeProductsGrams { get; set; }
}

