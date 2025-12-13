using FitnessProject.BLL.DTO.MealRecipe;

namespace FitnessProject.BLL.Services.Interfaces;

public interface IMealRecipeService : IService<Entities.MealRecipe, MealRecipeCreateDto, MealRecipeUpdateDto, MealRecipeResponseDto>
{
}

