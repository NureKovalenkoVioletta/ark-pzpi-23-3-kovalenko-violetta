using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FitnessProject.BLL.DTO.MacroNutrients;
using FitnessProject.BLL.Services;
using FitnessProject.Enums;
using FitnessProject.DAL.Interfaces;
using Moq;
using Xunit;

namespace FitnessProject.Tests.Services;

public class MealPlanGeneratorMacrosTests
{
    private MealPlanGeneratorService CreateService()
    {
        return new MealPlanGeneratorService(
            new Mock<IUserProfileRepository>().Object,
            new Mock<IRecipeRepository>().Object,
            new Mock<IProductRepository>().Object,
            new Mock<IDailyDietPlanRepository>().Object,
            new Mock<IMealRepository>().Object,
            new Mock<IMealRecipeRepository>().Object);
    }

    [Fact]
    public void DistributeCalories_ByMealTime_PercentagesAndSum()
    {
        var svc = CreateService();
        var method = typeof(MealPlanGeneratorService).GetMethod("DistributeCaloriesByMealTime", BindingFlags.NonPublic | BindingFlags.Instance);
        var total = 2000m;

        var result = (Dictionary<MealTime, decimal>)method!.Invoke(svc, new object[] { total })!;

        Assert.Equal(4, result.Count);
        Assert.Equal(550.0m, result[MealTime.Breakfast]);
        Assert.Equal(650.0m, result[MealTime.Lunch]);
        Assert.Equal(550.0m, result[MealTime.Dinner]);
        Assert.Equal(250.0m, result[MealTime.Snack]);
        Assert.Equal(total, result.Values.Sum());
    }

    [Fact]
    public void BalanceMacrosForMeals_ProportionalAndSumClose()
    {
        var svc = CreateService();
        var distMethod = typeof(MealPlanGeneratorService).GetMethod("DistributeCaloriesByMealTime", BindingFlags.NonPublic | BindingFlags.Instance);
        var balanceMethod = typeof(MealPlanGeneratorService).GetMethod("BalanceMacrosForMeals", BindingFlags.NonPublic | BindingFlags.Instance);

        var totalCalories = 2200m;
        var dailyMacros = new MacroNutrientsDto
        {
            Calories = totalCalories,
            ProteinGrams = 160m,
            FatGrams = 70m,
            CarbsGrams = 250m
        };

        var caloriesByMeal = (Dictionary<MealTime, decimal>)distMethod!.Invoke(svc, new object[] { totalCalories })!;
        var macrosByMeal = (Dictionary<MealTime, MacroNutrientsDto>)balanceMethod!.Invoke(svc, new object[] { caloriesByMeal, dailyMacros })!;

        Assert.Equal(4, macrosByMeal.Count);

        var proteinSum = macrosByMeal.Values.Sum(m => m.ProteinGrams);
        var fatSum = macrosByMeal.Values.Sum(m => m.FatGrams);
        var carbsSum = macrosByMeal.Values.Sum(m => m.CarbsGrams);
        var caloriesSum = macrosByMeal.Values.Sum(m => m.Calories);

        Assert.Equal(totalCalories, caloriesSum);
        Assert.InRange(proteinSum, dailyMacros.ProteinGrams - 0.2m, dailyMacros.ProteinGrams + 0.2m);
        Assert.InRange(fatSum, dailyMacros.FatGrams - 0.2m, dailyMacros.FatGrams + 0.2m);
        Assert.InRange(carbsSum, dailyMacros.CarbsGrams - 0.2m, dailyMacros.CarbsGrams + 0.2m);
    }
}

