using FitnessProject.BLL.DTO.RecipeProduct;

namespace FitnessProject.BLL.Services.Interfaces;

public interface IRecipeProductService : IService<Entities.RecipeProduct, RecipeProductCreateDto, RecipeProductUpdateDto, RecipeProductResponseDto>
{
    Task<RecipeProductDetailsDto?> GetRecipeProductDetailsByIdAsync(int recipeId, int productId);
}

