namespace FitnessProject.BLL.DTO.Product;

public class ProductResponseDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal CaloriesPer100g { get; set; }
    public decimal ProteinPer100g { get; set; }
    public decimal FatPer100g { get; set; }
    public decimal CarbsPer100g { get; set; }
    public string? Restriction { get; set; }
    public string? Allergens { get; set; }
    public string? Unit { get; set; }
}

