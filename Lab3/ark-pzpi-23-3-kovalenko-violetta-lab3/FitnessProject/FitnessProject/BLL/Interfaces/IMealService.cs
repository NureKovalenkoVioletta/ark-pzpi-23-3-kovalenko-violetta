using FitnessProject.BLL.DTO.Meal;

namespace FitnessProject.BLL.Services.Interfaces;

public interface IMealService : IService<Entities.Meal, MealCreateDto, MealUpdateDto, MealResponseDto>
{
    Task<MealDetailsDto?> GetMealDetailsByIdAsync(int id);
}

