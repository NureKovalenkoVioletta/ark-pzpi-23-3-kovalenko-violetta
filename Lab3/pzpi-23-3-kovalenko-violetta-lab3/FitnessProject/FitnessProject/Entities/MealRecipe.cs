namespace FitnessProject.Entities;

public class MealRecipe
{
    public int MealId { get; set; }
    public int RecipeId { get; set; }
    public string? PortionsMetadata { get; set; }

    public Meal Meal { get; set; } = null!;
    public Recipe Recipe { get; set; } = null!;
}

