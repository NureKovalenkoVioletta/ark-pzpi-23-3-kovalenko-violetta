using FitnessProject.BLL.DTO.MacroNutrients;
using FitnessProject.Enums;

namespace FitnessProject.BLL.Services.Helpers;

public static class MacroNutrientsCalculator
{
    private const decimal CALORIES_PER_GRAM_PROTEIN = 4m;
    private const decimal CALORIES_PER_GRAM_FAT = 9m;
    private const decimal CALORIES_PER_GRAM_CARBS = 4m;

    private const decimal PROTEIN_PERCENTAGE_MIN = 0.25m;
    private const decimal PROTEIN_PERCENTAGE_MAX = 0.30m;
    private const decimal PROTEIN_PERCENTAGE_AVG = 0.275m;
    private const decimal FAT_PERCENTAGE_MIN = 0.25m;
    private const decimal FAT_PERCENTAGE_MAX = 0.30m;
    private const decimal FAT_PERCENTAGE_AVG = 0.275m;

    private const decimal PROTEIN_PER_KG_MIN = 1.6m;
    private const decimal PROTEIN_PER_KG_MAX = 2.2m;
    private const decimal PROTEIN_PER_KG_AVG = 1.9m;
    private const decimal FAT_PER_KG_MIN = 0.8m;
    private const decimal FAT_PER_KG_MAX = 1.0m;
    private const decimal FAT_PER_KG_AVG = 0.9m;

    public static decimal CalculateCaloriesForWeightLoss(decimal tdee)
    {
        return tdee * 0.825m;
    }

    public static decimal CalculateCaloriesForWeightGain(decimal tdee)
    {
        return tdee * 1.125m;
    }

    public static decimal CalculateCaloriesForMaintenance(decimal tdee)
    {
        return tdee;
    }

    public static decimal CalculateCaloriesByGoal(decimal tdee, GoalType? goalType)
    {
        if (goalType == null)
        {
            return CalculateCaloriesForMaintenance(tdee);
        }

        return goalType switch
        {
            GoalType.WeightLoss => CalculateCaloriesForWeightLoss(tdee),
            GoalType.WeightGain => CalculateCaloriesForWeightGain(tdee),
            GoalType.WeightMaintenance => CalculateCaloriesForMaintenance(tdee),
            GoalType.HealthCorrection => CalculateCaloriesForMaintenance(tdee),
            _ => CalculateCaloriesForMaintenance(tdee)
        };
    }

    public static MacroNutrientsDto CalculateMacros(decimal calories, decimal weight, GoalType? goalType)
    {
        var proteinGrams = CalculateProteinGrams(calories, weight, goalType);
        var fatGrams = CalculateFatGrams(calories, weight, goalType);
        var carbsGrams = CalculateCarbsGrams(calories, proteinGrams, fatGrams);

        return new MacroNutrientsDto
        {
            Calories = calories,
            ProteinGrams = Math.Round(proteinGrams, 1),
            FatGrams = Math.Round(fatGrams, 1),
            CarbsGrams = Math.Round(carbsGrams, 1)
        };
    }

    private static decimal CalculateProteinGrams(decimal calories, decimal weight, GoalType? goalType)
    {
        var proteinFromWeight = weight * PROTEIN_PER_KG_AVG;
        var proteinFromCalories = (calories * PROTEIN_PERCENTAGE_AVG) / CALORIES_PER_GRAM_PROTEIN;

        return Math.Max(proteinFromWeight, proteinFromCalories);
    }

    private static decimal CalculateFatGrams(decimal calories, decimal weight, GoalType? goalType)
    {
        var fatFromWeight = weight * FAT_PER_KG_AVG;
        var fatFromCalories = (calories * FAT_PERCENTAGE_AVG) / CALORIES_PER_GRAM_FAT;

        return Math.Max(fatFromWeight, fatFromCalories);
    }

    private static decimal CalculateCarbsGrams(decimal calories, decimal proteinGrams, decimal fatGrams)
    {
        var proteinCalories = proteinGrams * CALORIES_PER_GRAM_PROTEIN;
        var fatCalories = fatGrams * CALORIES_PER_GRAM_FAT;
        var carbsCalories = calories - proteinCalories - fatCalories;

        if (carbsCalories < 0)
        {
            return 0;
        }

        return carbsCalories / CALORIES_PER_GRAM_CARBS;
    }
}

