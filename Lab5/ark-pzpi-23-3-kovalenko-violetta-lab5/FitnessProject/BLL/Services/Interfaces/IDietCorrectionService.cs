using FitnessProject.BLL.DTO.MacroNutrients;
using FitnessProject.BLL.DTO.Services;
using FitnessProject.Entities;

namespace FitnessProject.BLL.Services.Interfaces;

public interface IDietCorrectionService
{
    Task<List<Recommendation>> CheckAndSuggestCorrectionsAsync(int userId, int dailyDietPlanId);
    Task<DailyDietPlan> ApplyCorrectionAsync(int dailyDietPlanId, int recommendationId);

    MacroNutrientsDto CalculateCorrectedMacros(
        DailyDietPlan currentPlan,
        ActivityChangeResult activity,
        SleepQualityAnalysis sleep);

    Recommendation CreateCorrectionRecommendation(
        int userId,
        int? mealId,
        string reason,
        MacroNutrientsDto suggestedMacros,
        MacroNutrientsDto? currentMacros = null);

    string SuggestMenuChanges(DailyDietPlan currentPlan, MacroNutrientsDto newTargets);
}

