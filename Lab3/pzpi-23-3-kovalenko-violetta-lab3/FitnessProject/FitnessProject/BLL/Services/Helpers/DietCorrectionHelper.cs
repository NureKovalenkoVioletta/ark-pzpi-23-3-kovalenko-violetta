using FitnessProject.BLL.DTO.MacroNutrients;

namespace FitnessProject.BLL.Services.Helpers;

public static class DietCorrectionHelper
{
    public static decimal AdjustCaloriesForHighActivity(decimal currentCalories, decimal activityIncreasePercent)
    {
        if (activityIncreasePercent <= 0)
        {
            return currentCalories;
        }

        return currentCalories * (1 + activityIncreasePercent / 100m);
    }

    public static decimal AdjustCaloriesForLowActivity(decimal currentCalories, decimal activityDecreasePercent)
    {
        if (activityDecreasePercent <= 0)
        {
            return currentCalories;
        }

        var adjusted = currentCalories * (1 - activityDecreasePercent / 100m);
        return adjusted < 0 ? 0 : adjusted;
    }

    public static MacroNutrientsDto AdjustMacrosForSleepDeprivation(
        MacroNutrientsDto currentMacros,
        decimal carbDecreasePercent = 0.10m,
        decimal proteinIncreasePercent = 0.10m,
        decimal fatIncreasePercent = 0m)
    {
        var protein = ApplyIncrease(currentMacros.ProteinGrams, proteinIncreasePercent);
        var carbs = ApplyDecrease(currentMacros.CarbsGrams, carbDecreasePercent);
        var fat = ApplyIncrease(currentMacros.FatGrams, fatIncreasePercent);

        return RecalculateCalories(protein, fat, carbs);
    }

    public static MacroNutrientsDto AdjustForAbnormalHeartRate(
        MacroNutrientsDto currentMacros,
        decimal calorieDecreasePercent = 0.10m,
        decimal proteinIncreasePercent = 0.05m,
        decimal carbDecreasePercent = 0.05m)
    {
        var targetCalories = AdjustCaloriesForLowActivity(currentMacros.Calories, calorieDecreasePercent);

        var protein = ApplyIncrease(currentMacros.ProteinGrams, proteinIncreasePercent);
        var carbs = ApplyDecrease(currentMacros.CarbsGrams, carbDecreasePercent);
        var fat = currentMacros.FatGrams;

        var adjusted = RecalculateCalories(protein, fat, carbs);

        if (adjusted.Calories > targetCalories && adjusted.Calories > 0)
        {
            var ratio = targetCalories / adjusted.Calories;
            adjusted.ProteinGrams = Math.Round(adjusted.ProteinGrams * ratio, 1);
            adjusted.FatGrams = Math.Round(adjusted.FatGrams * ratio, 1);
            adjusted.CarbsGrams = Math.Round(adjusted.CarbsGrams * ratio, 1);
            adjusted.Calories = targetCalories;
        }

        return adjusted;
    }

    public static MacroNutrientsDto AdjustMacrosForHighActivity(
        MacroNutrientsDto currentMacros,
        decimal carbIncreasePercent = 0.10m,
        decimal fatDecreasePercent = 0.05m)
    {
        var protein = currentMacros.ProteinGrams;
        var fatDecreased = ApplyDecrease(currentMacros.FatGrams, fatDecreasePercent);
        var carbsIncreased = ApplyIncrease(currentMacros.CarbsGrams, carbIncreasePercent);

        var normalized = NormalizeToCalories(protein, fatDecreased, carbsIncreased, currentMacros.Calories);
        // Сохраняем направление изменений: жиры не выше, чем уменьшенный вариант, карбы не ниже увеличенного.
        normalized.FatGrams = Math.Min(normalized.FatGrams, fatDecreased);
        normalized.CarbsGrams = Math.Max(normalized.CarbsGrams, carbsIncreased);
        normalized.ProteinGrams = protein;
        return normalized;
    }

    public static MacroNutrientsDto AdjustMacrosForLowActivity(
        MacroNutrientsDto currentMacros,
        decimal carbDecreasePercent = 0.10m,
        decimal proteinIncreasePercent = 0.10m)
    {
        var protein = ApplyIncrease(currentMacros.ProteinGrams, proteinIncreasePercent);
        var fat = currentMacros.FatGrams;
        var carbsDecreased = ApplyDecrease(currentMacros.CarbsGrams, carbDecreasePercent);

        var normalized = NormalizeToCalories(protein, fat, carbsDecreased, currentMacros.Calories);
        normalized.ProteinGrams = Math.Max(normalized.ProteinGrams, protein);
        normalized.CarbsGrams = Math.Min(normalized.CarbsGrams, carbsDecreased);
        normalized.FatGrams = fat;
        return normalized;
    }

    public static MacroNutrientsDto AdjustMacrosForRecovery(
        MacroNutrientsDto currentMacros,
        decimal proteinIncreasePercent = 0.10m,
        decimal fatIncreasePercent = 0.05m,
        decimal carbDecreasePercent = 0.05m)
    {
        var protein = ApplyIncrease(currentMacros.ProteinGrams, proteinIncreasePercent);
        var fatIncreased = ApplyIncrease(currentMacros.FatGrams, fatIncreasePercent);
        var carbsDecreased = ApplyDecrease(currentMacros.CarbsGrams, carbDecreasePercent);

        var normalized = NormalizeToCalories(protein, fatIncreased, carbsDecreased, currentMacros.Calories);
        normalized.ProteinGrams = Math.Max(normalized.ProteinGrams, protein);
        normalized.FatGrams = Math.Max(normalized.FatGrams, fatIncreased);
        normalized.CarbsGrams = Math.Min(normalized.CarbsGrams, carbsDecreased);
        return normalized;
    }

    private static MacroNutrientsDto RecalculateCalories(decimal proteinGrams, decimal fatGrams, decimal carbsGrams)
    {
        var calories = proteinGrams * 4m + fatGrams * 9m + carbsGrams * 4m;

        return new MacroNutrientsDto
        {
            Calories = Math.Round(calories, 1),
            ProteinGrams = Math.Round(proteinGrams, 1),
            FatGrams = Math.Round(fatGrams, 1),
            CarbsGrams = Math.Round(carbsGrams, 1)
        };
    }

    private static decimal ApplyIncrease(decimal value, decimal percent)
    {
        if (percent <= 0)
        {
            return value;
        }
        return value * (1 + percent);
    }

    private static decimal ApplyDecrease(decimal value, decimal percent)
    {
        if (percent <= 0)
        {
            return value;
        }
        var decreased = value * (1 - percent);
        return decreased < 0 ? 0 : decreased;
    }

    private static MacroNutrientsDto NormalizeToCalories(decimal protein, decimal fat, decimal carbs, decimal targetCalories)
    {
        var adjusted = RecalculateCalories(protein, fat, carbs);

        if (adjusted.Calories <= 0 || targetCalories <= 0)
        {
            return adjusted;
        }

        var ratio = targetCalories / adjusted.Calories;
        return new MacroNutrientsDto
        {
            Calories = Math.Round(targetCalories, 1),
            ProteinGrams = Math.Round(adjusted.ProteinGrams * ratio, 1),
            FatGrams = Math.Round(adjusted.FatGrams * ratio, 1),
            CarbsGrams = Math.Round(adjusted.CarbsGrams * ratio, 1)
        };
    }
}

