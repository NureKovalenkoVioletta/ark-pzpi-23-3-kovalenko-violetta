using FitnessProject.BLL.DTO.Recipe;
using FitnessProject.BLL.DTO.Product;

namespace FitnessProject.BLL.DTO.RecipeProduct;

public class RecipeProductDetailsDto
{
    public int RecipeId { get; set; }
    public int ProductId { get; set; }
    public decimal QuantityGrams { get; set; }
    
    public RecipeResponseDto Recipe { get; set; } = null!;
    public ProductResponseDto Product { get; set; } = null!;
}

