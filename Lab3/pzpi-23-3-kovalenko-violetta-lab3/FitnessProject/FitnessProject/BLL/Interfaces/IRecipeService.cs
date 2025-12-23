using FitnessProject.BLL.DTO.Recipe;

namespace FitnessProject.BLL.Services.Interfaces;

public interface IRecipeService : IService<Entities.Recipe, RecipeCreateDto, RecipeUpdateDto, RecipeResponseDto>
{
    Task<RecipeDetailsDto?> GetRecipeDetailsByIdAsync(int id);
}

