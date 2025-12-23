using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FitnessProject.BLL.DTO.MacroNutrients;
using FitnessProject.BLL.Services;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;
using FitnessProject.Enums;
using FitnessProject.Resources;
using Moq;
using Microsoft.Extensions.Localization;
using Xunit;

namespace FitnessProject.Tests.Services;

public class RebalanceMealsTests
{
    [Fact]
    public async Task RebalanceMeals_ScalesMealTargetsAndPortions()
    {
        var svc = CreateService(
            out var activity,
            out var sleep,
            out var plans,
            out var recos,
            out var users,
            out var meals,
            out var mealRecipes,
            out var recipeRepo);

        var plan = new DailyDietPlan
        {
            DailyDietPlanId = 1,
            DailyPlanCalories = 2000m,
            DailyPlanProtein = 120m,
            DailyPlanFat = 60m,
            DailyPlanCarbs = 200m,
            Meals = new List<Meal>
            {
                new()
                {
                    MealId = 10,
                    MealTargetCalories = 500m,
                    MealTargetProtein = 30m,
                    MealTargetFat = 15m,
                    MealTargetCarbs = 50m,
                    MealRecipes = new List<MealRecipe>
                    {
                        new() { MealId = 10, RecipeId = 1000, PortionsMetadata = "old" }
                    }
                }
            }
        };

        var oldMacros = new MacroNutrientsDto { Calories = 2000m, ProteinGrams = 120m, FatGrams = 60m, CarbsGrams = 200m };
        var newMacros = new MacroNutrientsDto { Calories = 2400m, ProteinGrams = 140m, FatGrams = 70m, CarbsGrams = 240m };

        recipeRepo.Setup(r => r.GetRecipeDetailsByIdAsync(1000))
            .ReturnsAsync(new Recipe
            {
                RecipeId = 1000,
                RecipeProducts = new List<RecipeProduct>
                {
                    new() { ProductId = 1,  QuantityGrams = 100 }
                }
            });

        meals.Setup(m => m.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Meal, bool>>>()))
            .ReturnsAsync(plan.Meals.ToList());
        meals.Setup(m => m.GetMealDetailsByIdAsync(10))
            .ReturnsAsync(plan.Meals.First());
        meals.Setup(m => m.UpdateAsync(It.IsAny<Meal>())).Returns(Task.CompletedTask);
        mealRecipes.Setup(mr => mr.UpdateAsync(It.IsAny<MealRecipe>())).Returns(Task.CompletedTask);

        // Act
        await svc.RebalanceMealsAsync(plan, oldMacros, newMacros);

        // Assert targets scaled by ratios
        var updatedMeal = plan.Meals.First();
        Assert.InRange(updatedMeal.MealTargetCalories, 530m, 545m); // 35*4 + 17.5*9 + 60*4 = 537.5
        Assert.InRange(updatedMeal.MealTargetProtein, 34m, 36m);
        Assert.InRange(updatedMeal.MealTargetFat, 17m, 18m);
        Assert.InRange(updatedMeal.MealTargetCarbs, 59m, 61m);

        mealRecipes.Verify(mr => mr.UpdateAsync(It.Is<MealRecipe>(x => x.MealId == 10 && x.RecipeId == 1000)), Times.Once);
        meals.Verify(m => m.UpdateAsync(It.Is<Meal>(x => x.MealId == 10)), Times.Once);
    }

    [Fact]
    public async Task RebalanceMeals_NoRecipeDetails_EmptyMetadata()
    {
        var svc = CreateService(
            out var activity,
            out var sleep,
            out var plans,
            out var recos,
            out var users,
            out var meals,
            out var mealRecipes,
            out var recipeRepo);

        var plan = new DailyDietPlan
        {
            DailyDietPlanId = 2,
            DailyPlanCalories = 2000m,
            DailyPlanProtein = 120m,
            DailyPlanFat = 60m,
            DailyPlanCarbs = 200m,
            Meals = new List<Meal>
            {
                new()
                {
                    MealId = 20,
                    MealTargetCalories = 500m,
                    MealTargetProtein = 30m,
                    MealTargetFat = 15m,
                    MealTargetCarbs = 50m,
                    MealRecipes = new List<MealRecipe>
                    {
                        new() { MealId = 20, RecipeId = 2000, PortionsMetadata = "old" }
                    }
                }
            }
        };

        var oldMacros = new MacroNutrientsDto { Calories = 2000m, ProteinGrams = 120m, FatGrams = 60m, CarbsGrams = 200m };
        var newMacros = new MacroNutrientsDto { Calories = 2400m, ProteinGrams = 140m, FatGrams = 70m, CarbsGrams = 240m };

        recipeRepo.Setup(r => r.GetRecipeDetailsByIdAsync(2000)).ReturnsAsync((Recipe?)null);
        meals.Setup(m => m.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Meal, bool>>>()))
            .ReturnsAsync(plan.Meals.ToList());
        meals.Setup(m => m.GetMealDetailsByIdAsync(20))
            .ReturnsAsync(plan.Meals.First());
        meals.Setup(m => m.UpdateAsync(It.IsAny<Meal>())).Returns(Task.CompletedTask);
        mealRecipes.Setup(mr => mr.UpdateAsync(It.IsAny<MealRecipe>())).Returns(Task.CompletedTask);

        await svc.RebalanceMealsAsync(plan, oldMacros, newMacros);

        var updated = plan.Meals.First().MealRecipes.First();
        Assert.Equal(string.Empty, updated.PortionsMetadata);
    }

    private DietCorrectionService CreateService(
        out Mock<IActivityMonitorService> activity,
        out Mock<ISleepAnalysisService> sleep,
        out Mock<IDailyDietPlanRepository> plans,
        out Mock<IRecommendationRepository> recos,
        out Mock<IUserRepository> users,
        out Mock<IMealRepository> meals,
        out Mock<IMealRecipeRepository> mealRecipes,
        out Mock<IRecipeRepository> recipeRepo)
    {
        activity = new Mock<IActivityMonitorService>();
        sleep = new Mock<ISleepAnalysisService>();
        plans = new Mock<IDailyDietPlanRepository>();
        recos = new Mock<IRecommendationRepository>();
        users = new Mock<IUserRepository>();
        meals = new Mock<IMealRepository>();
        mealRecipes = new Mock<IMealRecipeRepository>();
        recipeRepo = new Mock<IRecipeRepository>();

        var localizer = new Mock<IStringLocalizer<SharedResources>>();
        localizer
            .Setup(l => l[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));
        localizer
            .Setup(l => l[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string key, object[] args) => new LocalizedString(key, key));

        var units = new Mock<IUnitConversionService>();
        units.Setup(u => u.DeterminePreferredUnits(It.IsAny<string>())).Returns(PreferredUnits.Metric);
        units.Setup(u => u.ConvertWeight(It.IsAny<decimal>(), It.IsAny<PreferredUnits>(), It.IsAny<PreferredUnits>(), It.IsAny<int>()))
            .Returns<decimal, PreferredUnits, PreferredUnits, int>((v, _, __, ___) => v);
        units.Setup(u => u.ConvertVolume(It.IsAny<decimal>(), It.IsAny<PreferredUnits>(), It.IsAny<PreferredUnits>(), It.IsAny<int>()))
            .Returns<decimal, PreferredUnits, PreferredUnits, int>((v, _, __, ___) => v);

        return new DietCorrectionService(
            activity.Object,
            sleep.Object,
            plans.Object,
            recos.Object,
            users.Object,
            meals.Object,
            mealRecipes.Object,
            recipeRepo.Object,
            localizer.Object,
            units.Object);
    }
}

