using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FitnessProject.BLL.DTO.MacroNutrients;
using FitnessProject.BLL.DTO.Services;
using FitnessProject.BLL.Services;
using FitnessProject.BLL.Services.Helpers;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;
using FitnessProject.Enums;
using FitnessProject.Resources;
using Moq;
using Microsoft.Extensions.Localization;
using Xunit;

namespace FitnessProject.Tests.Services;

public class DietCorrectionServiceTests
{
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

    [Fact]
    public async Task CheckAndSuggest_NoChanges_ReturnsEmpty()
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

        var plan = Plan();
        plans.Setup(p => p.GetDailyDietPlanDetailsByIdAsync(It.IsAny<int>())).ReturnsAsync(plan);
        activity.Setup(a => a.CheckActivityChangesAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new ActivityChangeResult());
        sleep.Setup(s => s.AnalyzeSleepQualityAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<int>()))
            .ReturnsAsync(new SleepQualityAnalysis());
        users.Setup(u => u.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new User { UserId = 1, Locale = "en" });

        var result = await svc.CheckAndSuggestCorrectionsAsync(1, 1);

        Assert.Empty(result);
    }

    [Fact]
    public async Task CheckAndSuggest_WithChanges_ReturnsRecommendation()
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

        var plan = Plan();
        plans.Setup(p => p.GetDailyDietPlanDetailsByIdAsync(It.IsAny<int>())).ReturnsAsync(plan);
        activity.Setup(a => a.CheckActivityChangesAsync(It.IsAny<int>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new ActivityChangeResult { StepsSpike = true });
        sleep.Setup(s => s.AnalyzeSleepQualityAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<int>()))
            .ReturnsAsync(new SleepQualityAnalysis());
        users.Setup(u => u.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new User { UserId = 1, Locale = "en" });
        recos.Setup(r => r.AddAsync(It.IsAny<Recommendation>()))
            .ReturnsAsync((Recommendation r) =>
            {
                r.RecommendationId = 10;
                return r;
            });

        var result = await svc.CheckAndSuggestCorrectionsAsync(1, 1);

        Assert.Single(result);
        Assert.Equal(RecommendationType.DietCorrection, result.First().RecommendationType);
    }

    [Fact]
    public async Task ApplyCorrection_UpdatesPlanAndSetsIsCorrected()
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

        var plan = Plan();
        plans.Setup(p => p.GetDailyDietPlanDetailsByIdAsync(It.IsAny<int>())).ReturnsAsync(plan);
        recos.Setup(r => r.GetRecommendationDetailsByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Recommendation
            {
                RecommendationId = 10,
                RecommendationType = RecommendationType.DietCorrection,
                RecommendationPayload = System.Text.Json.JsonSerializer.Serialize(new MacroNutrientsDto
                {
                    Calories = 1800m,
                    ProteinGrams = 140m,
                    FatGrams = 55m,
                    CarbsGrams = 180m
                })
            });

        recos.Setup(r => r.UpdateAsync(It.IsAny<Recommendation>())).Returns(Task.CompletedTask);
        plans.Setup(p => p.UpdateAsync(It.IsAny<DailyDietPlan>())).Returns(Task.CompletedTask);
        mealRecipes.Setup(mr => mr.UpdateAsync(It.IsAny<MealRecipe>())).Returns(Task.CompletedTask);
        meals.Setup(m => m.UpdateAsync(It.IsAny<Meal>())).Returns(Task.CompletedTask);

        var result = await svc.ApplyCorrectionAsync(1, 10);

        Assert.True(result.IsCorrected);
        Assert.Equal(1800m, result.DailyPlanCalories);
    }

    [Fact]
    public void CalculateCorrectedMacros_HeartRateAnomaly_AdjustsDown()
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

        var plan = Plan();
        var activityResult = new ActivityChangeResult { HeartRateAnomaly = true };
        var sleepResult = new SleepQualityAnalysis { IsSleepDeprived = false };

        var corrected = svc.CalculateCorrectedMacros(plan, activityResult, sleepResult);

        Assert.True(corrected.Calories < plan.DailyPlanCalories);
        Assert.True(corrected.ProteinGrams > plan.DailyPlanProtein);
        Assert.True(corrected.CarbsGrams < plan.DailyPlanCarbs);
    }

    [Fact]
    public void CalculateCorrectedMacros_SleepDeprived_AdjustsMacros()
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

        var plan = Plan();
        var activityResult = new ActivityChangeResult();
        var sleepResult = new SleepQualityAnalysis { IsSleepDeprived = true };

        var corrected = svc.CalculateCorrectedMacros(plan, activityResult, sleepResult);

        Assert.True(corrected.ProteinGrams > plan.DailyPlanProtein);
        Assert.True(corrected.CarbsGrams < plan.DailyPlanCarbs);
    }

    [Fact]
    public async Task ApplyCorrection_AlreadyCorrected_PersistsAndStaysCorrected()
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

        var plan = Plan(isCorrected: true);
        plans.Setup(p => p.GetDailyDietPlanDetailsByIdAsync(It.IsAny<int>())).ReturnsAsync(plan);
        recos.Setup(r => r.GetRecommendationDetailsByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Recommendation
            {
                RecommendationId = 11,
                RecommendationType = RecommendationType.DietCorrection,
                RecommendationPayload = System.Text.Json.JsonSerializer.Serialize(new MacroNutrientsDto
                {
                    Calories = 1700m,
                    ProteinGrams = 130m,
                    FatGrams = 50m,
                    CarbsGrams = 180m
                })
            });

        recos.Setup(r => r.UpdateAsync(It.IsAny<Recommendation>())).Returns(Task.CompletedTask);
        plans.Setup(p => p.UpdateAsync(It.IsAny<DailyDietPlan>())).Returns(Task.CompletedTask);
        mealRecipes.Setup(mr => mr.UpdateAsync(It.IsAny<MealRecipe>())).Returns(Task.CompletedTask);
        meals.Setup(m => m.UpdateAsync(It.IsAny<Meal>())).Returns(Task.CompletedTask);

        var result = await svc.ApplyCorrectionAsync(1, 11);

        Assert.True(result.IsCorrected);
        Assert.Equal(1700m, result.DailyPlanCalories);
    }

    private static DailyDietPlan Plan() =>
        new()
        {
            DailyDietPlanId = 1,
            UserId = 1,
            DailyPlanCalories = 2000m,
            DailyPlanProtein = 120m,
            DailyPlanFat = 60m,
            DailyPlanCarbs = 200m,
            DailyPlanCreatedAt = DateTime.UtcNow,
            User = new User { UserId = 1, Locale = "en" }
        };

    private static DailyDietPlan Plan(bool isCorrected) =>
        new()
        {
            DailyDietPlanId = 1,
            UserId = 1,
            DailyPlanCalories = 2000m,
            DailyPlanProtein = 120m,
            DailyPlanFat = 60m,
            DailyPlanCarbs = 200m,
            DailyPlanCreatedAt = DateTime.UtcNow,
            User = new User { UserId = 1, Locale = "en" },
            IsCorrected = isCorrected
        };
}

