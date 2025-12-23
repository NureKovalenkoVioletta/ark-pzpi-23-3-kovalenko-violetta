using FitnessProject.BLL.DTO.MacroNutrients;
using FitnessProject.BLL.Services.Helpers;
using Xunit;

namespace FitnessProject.Tests.Services;

public class DietCorrectionHelperTests
{
    [Fact]
    public void AdjustCalories_HighActivity_Increases()
    {
        var result = DietCorrectionHelper.AdjustCaloriesForHighActivity(2000m, 10m);
        Assert.Equal(2200m, result);
    }

    [Fact]
    public void AdjustCalories_LowActivity_DecreasesAndClamps()
    {
        var result = DietCorrectionHelper.AdjustCaloriesForLowActivity(2000m, 20m);
        Assert.Equal(1600m, result);

        var zero = DietCorrectionHelper.AdjustCaloriesForLowActivity(100m, 200m);
        Assert.Equal(0m, zero);
    }

    [Fact]
    public void AdjustMacros_SleepDeprivation_ShiftsProteinUpCarbsDown()
    {
        var current = new MacroNutrientsDto { Calories = 2000m, ProteinGrams = 120m, FatGrams = 60m, CarbsGrams = 200m };
        var adjusted = DietCorrectionHelper.AdjustMacrosForSleepDeprivation(current, carbDecreasePercent: 0.1m, proteinIncreasePercent: 0.1m);

        Assert.True(adjusted.ProteinGrams > current.ProteinGrams);
        Assert.True(adjusted.CarbsGrams < current.CarbsGrams);
        Assert.True(adjusted.Calories > 0);
    }

    [Fact]
    public void AdjustForAbnormalHeartRate_DecreasesCaloriesAndCarbsIncreasesProtein()
    {
        var current = new MacroNutrientsDto { Calories = 2000m, ProteinGrams = 120m, FatGrams = 60m, CarbsGrams = 200m };
        var adjusted = DietCorrectionHelper.AdjustForAbnormalHeartRate(current, calorieDecreasePercent: 0.1m, proteinIncreasePercent: 0.05m, carbDecreasePercent: 0.05m);

        Assert.True(adjusted.Calories <= 2000m);
        Assert.True(adjusted.ProteinGrams > current.ProteinGrams);
        Assert.True(adjusted.CarbsGrams < current.CarbsGrams);
    }

    [Fact]
    public void AdjustMacros_HighActivity_NormalizesCalories()
    {
        var current = new MacroNutrientsDto { Calories = 2000m, ProteinGrams = 120m, FatGrams = 60m, CarbsGrams = 200m };
        var adjusted = DietCorrectionHelper.AdjustMacrosForHighActivity(current, carbIncreasePercent: 0.1m, fatDecreasePercent: 0.05m);

        Assert.Equal(2000m, adjusted.Calories);
        Assert.True(adjusted.CarbsGrams > current.CarbsGrams);
        Assert.True(adjusted.FatGrams < current.FatGrams);
    }

    [Fact]
    public void AdjustMacros_LowActivity_NormalizesCalories()
    {
        var current = new MacroNutrientsDto { Calories = 2000m, ProteinGrams = 120m, FatGrams = 60m, CarbsGrams = 200m };
        var adjusted = DietCorrectionHelper.AdjustMacrosForLowActivity(current, carbDecreasePercent: 0.1m, proteinIncreasePercent: 0.1m);

        Assert.Equal(2000m, adjusted.Calories);
        Assert.True(adjusted.CarbsGrams < current.CarbsGrams);
        Assert.True(adjusted.ProteinGrams > current.ProteinGrams);
    }

    [Fact]
    public void AdjustMacros_Recovery_NormalizesCalories()
    {
        var current = new MacroNutrientsDto { Calories = 2000m, ProteinGrams = 120m, FatGrams = 60m, CarbsGrams = 200m };
        var adjusted = DietCorrectionHelper.AdjustMacrosForRecovery(current, proteinIncreasePercent: 0.1m, fatIncreasePercent: 0.05m, carbDecreasePercent: 0.05m);

        Assert.Equal(2000m, adjusted.Calories);
        Assert.True(adjusted.ProteinGrams > current.ProteinGrams);
        Assert.True(adjusted.FatGrams > current.FatGrams);
        Assert.True(adjusted.CarbsGrams < current.CarbsGrams);
    }

    [Fact]
    public void NormalizeToCalories_KeepsTargetCalories()
    {
        var adjusted = DietCorrectionHelper.AdjustMacrosForHighActivity(
            new MacroNutrientsDto { Calories = 1800m, ProteinGrams = 100m, FatGrams = 50m, CarbsGrams = 180m },
            carbIncreasePercent: 0.2m,
            fatDecreasePercent: 0.1m);

        Assert.Equal(1800m, adjusted.Calories);
    }
}

