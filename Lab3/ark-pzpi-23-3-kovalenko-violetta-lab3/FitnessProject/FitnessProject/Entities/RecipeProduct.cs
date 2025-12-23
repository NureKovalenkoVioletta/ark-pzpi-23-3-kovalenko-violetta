namespace FitnessProject.Entities;

public class RecipeProduct
{
    public int RecipeId { get; set; }
    public int ProductId { get; set; }
    public decimal QuantityGrams { get; set; }

    public Recipe Recipe { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

