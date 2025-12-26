namespace FitnessProject.Entities;

public class Recipe
{
    public int RecipeId { get; set; }
    public string RecipeName { get; set; } = string.Empty;
    public string RecipeInstructions { get; set; } = string.Empty;
    public decimal RecipeCaloriesPerPortion { get; set; }
    public decimal RecipeFatPerPortion { get; set; }
    public decimal RecipeCarbsPerPortion { get; set; }
    public decimal RecipeProteinPerPortion { get; set; }
    public decimal RecipeProductsGrams { get; set; }

    public ICollection<MealRecipe> MealRecipes { get; set; } = new List<MealRecipe>();
    public ICollection<RecipeProduct> RecipeProducts { get; set; } = new List<RecipeProduct>();
}

