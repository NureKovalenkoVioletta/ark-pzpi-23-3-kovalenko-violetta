using FitnessProject.BLL.DTO.MacroNutrients;
using FitnessProject.Entities;

namespace FitnessProject.BLL.Services.Interfaces;

public interface IMealPlanGeneratorService
{
    Task<DailyDietPlan> GenerateMealPlanAsync(int userId, DateTime date, int? templateDietPlanId = null);
}

