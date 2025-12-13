using FitnessProject.BLL.DTO.RecipeProduct;

namespace FitnessProject.BLL.Services.Interfaces;

public interface IRecipeProductService : IService<Entities.RecipeProduct, RecipeProductCreateDto, RecipeProductUpdateDto, RecipeProductResponseDto>
{
}

