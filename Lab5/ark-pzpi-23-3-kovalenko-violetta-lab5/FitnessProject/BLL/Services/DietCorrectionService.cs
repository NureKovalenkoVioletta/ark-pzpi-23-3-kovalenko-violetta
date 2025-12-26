using System.Text.Json;
using FitnessProject.BLL.DTO.MacroNutrients;
using FitnessProject.BLL.DTO.Services;
using FitnessProject.BLL.Services.Helpers;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;
using FitnessProject.Enums;
using FitnessProject.Resources;
using Microsoft.Extensions.Localization;

namespace FitnessProject.BLL.Services;

public class DietCorrectionService : IDietCorrectionService
{
    private readonly IActivityMonitorService _activityMonitorService;
    private readonly ISleepAnalysisService _sleepAnalysisService;
    private readonly IDailyDietPlanRepository _dailyDietPlanRepository;
    private readonly IRecommendationRepository _recommendationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMealRepository _mealRepository;
    private readonly IMealRecipeRepository _mealRecipeRepository;
    private readonly IRecipeRepository _recipeRepository;
    private readonly IStringLocalizer<SharedResources> _localizer;
    private readonly IUnitConversionService _unitConversionService;

    private const decimal HighActivityCarbIncrease = 0.10m;
    private const decimal HighActivityFatDecrease = 0.05m;
    private const decimal HighActivityCaloriesIncreasePercent = 10m;

    private const decimal LowActivityCarbDecrease = 0.10m;
    private const decimal LowActivityProteinIncrease = 0.10m;
    private const decimal LowActivityCaloriesDecreasePercent = 5m;

    private const decimal TrainingIntensitySpikeThreshold = 0.20m; // 20%
    private const decimal LowStepsThreshold = -0.30m; // -30%

    public DietCorrectionService(
        IActivityMonitorService activityMonitorService,
        ISleepAnalysisService sleepAnalysisService,
        IDailyDietPlanRepository dailyDietPlanRepository,
        IRecommendationRepository recommendationRepository,
        IUserRepository userRepository,
        IMealRepository mealRepository,
        IMealRecipeRepository mealRecipeRepository,
        IRecipeRepository recipeRepository,
        IStringLocalizer<SharedResources> localizer,
        IUnitConversionService unitConversionService)
    {
        _activityMonitorService = activityMonitorService;
        _sleepAnalysisService = sleepAnalysisService;
        _dailyDietPlanRepository = dailyDietPlanRepository;
        _recommendationRepository = recommendationRepository;
        _userRepository = userRepository;
        _mealRepository = mealRepository;
        _mealRecipeRepository = mealRecipeRepository;
        _recipeRepository = recipeRepository;
        _localizer = localizer;
        _unitConversionService = unitConversionService;
    }

    public async Task<List<Recommendation>> CheckAndSuggestCorrectionsAsync(int userId, int dailyDietPlanId)
    {
        var plan = await _dailyDietPlanRepository.GetDailyDietPlanDetailsByIdAsync(dailyDietPlanId);
        if (plan == null || plan.UserId != userId)
        {
            return new List<Recommendation>();
        }

        var locale = await ResolveLocale(userId);
        var activity = await _activityMonitorService.CheckActivityChangesAsync(userId, plan.DailyPlanCreatedAt);
        var sleep = await _sleepAnalysisService.AnalyzeSleepQualityAsync(userId, plan.DailyPlanCreatedAt);

        var corrected = CalculateCorrectedMacros(plan, activity, sleep);

        // Если изменений нет — рекомендаций не создаём
        if (!HasMeaningfulChange(plan, corrected))
        {
            return new List<Recommendation>();
        }

        var reason = BuildReason(activity, sleep, locale);
        var recommendation = CreateCorrectionRecommendation(
            userId,
            null,
            reason,
            corrected,
            ExtractCurrentMacros(plan));

        var saved = await _recommendationRepository.AddAsync(recommendation);
        return new List<Recommendation> { saved };
    }

    public async Task<DailyDietPlan> ApplyCorrectionAsync(int dailyDietPlanId, int recommendationId)
    {
        var plan = await _dailyDietPlanRepository.GetDailyDietPlanDetailsByIdAsync(dailyDietPlanId);
        if (plan == null)
        {
            throw new ArgumentException($"DailyDietPlan {dailyDietPlanId} not found");
        }

        var recommendation = await _recommendationRepository.GetRecommendationDetailsByIdAsync(recommendationId);
        if (recommendation == null || recommendation.RecommendationType != RecommendationType.DietCorrection)
        {
            throw new ArgumentException($"Recommendation {recommendationId} not found or invalid type");
        }

        if (string.IsNullOrWhiteSpace(recommendation.RecommendationPayload))
        {
            throw new ArgumentException("Recommendation payload is empty");
        }

        var suggested = JsonSerializer.Deserialize<MacroNutrientsDto>(recommendation.RecommendationPayload);
        if (suggested == null)
        {
            throw new ArgumentException("Failed to parse recommendation payload");
        }

        var oldMacros = ExtractCurrentMacros(plan);

        plan.DailyPlanCalories = suggested.Calories;
        plan.DailyPlanProtein = suggested.ProteinGrams;
        plan.DailyPlanFat = suggested.FatGrams;
        plan.DailyPlanCarbs = suggested.CarbsGrams;
        plan.IsCorrected = true;

        recommendation.RecommendationStatus = RecommendationStatus.Applied;
        await _recommendationRepository.UpdateAsync(recommendation);
        await _dailyDietPlanRepository.UpdateAsync(plan);

        try
        {
            await RebalanceMealsAsync(plan, oldMacros, suggested);
        }
        catch
        {
            // Не прерываем основной сценарий, если пересборка меню не удалась
        }

        return plan;
    }

    public MacroNutrientsDto CalculateCorrectedMacros(
        DailyDietPlan currentPlan,
        ActivityChangeResult activity,
        SleepQualityAnalysis sleep)
    {
        var macros = ExtractCurrentMacros(currentPlan);

        // Активность высокая: шаги >30% или интенсивность выросла
        if (activity.StepsSpike ||
            (activity.TrainingIntensityChangePercent.HasValue &&
             activity.TrainingIntensityChangePercent.Value > TrainingIntensitySpikeThreshold))
        {
            macros = DietCorrectionHelper.AdjustMacrosForHighActivity(macros, HighActivityCarbIncrease, HighActivityFatDecrease);
            macros.Calories = DietCorrectionHelper.AdjustCaloriesForHighActivity(macros.Calories, HighActivityCaloriesIncreasePercent);
        }
        // Активность низкая: шаги < -30% от среднего или интенсивность упала
        else if (activity.StepsChangePercent.HasValue &&
                 activity.StepsChangePercent.Value < LowStepsThreshold)
        {
            macros = DietCorrectionHelper.AdjustMacrosForLowActivity(macros, LowActivityCarbDecrease, LowActivityProteinIncrease);
            macros.Calories = DietCorrectionHelper.AdjustCaloriesForLowActivity(macros.Calories, LowActivityCaloriesDecreasePercent);
        }

        // Недосып
        if (sleep.IsSleepDeprived)
        {
            macros = DietCorrectionHelper.AdjustMacrosForSleepDeprivation(macros, 0.10m, 0.10m, 0m);
        }

        // Аномальный пульс
        if (activity.HeartRateAnomaly)
        {
            macros = DietCorrectionHelper.AdjustForAbnormalHeartRate(macros, 0.10m, 0.05m, 0.05m);
        }

        return macros;
    }

    public Recommendation CreateCorrectionRecommendation(
        int userId,
        int? mealId,
        string reason,
        MacroNutrientsDto suggestedMacros,
        MacroNutrientsDto? currentMacros = null)
    {
        var payload = JsonSerializer.Serialize(suggestedMacros);

        return new Recommendation
        {
            MealInstanceId = mealId,
            RecommendationCreatedAt = DateTime.UtcNow,
            RecommendationType = RecommendationType.DietCorrection,
            RecommendationPayload = payload,
            RecommendationStatus = RecommendationStatus.New
        };
    }

    public string SuggestMenuChanges(DailyDietPlan currentPlan, MacroNutrientsDto newTargets)
    {
        var deltas = BuildDeltas(currentPlan, newTargets);

        var locale = currentPlan.User?.Locale ?? "en";
        var lang = GetLanguage(locale);
        var units = _unitConversionService.DeterminePreferredUnits(locale);

        var templateKey = "Recommendations.DietCorrection.Suggest";
        var localized = _localizer[templateKey];
        var templateValue = localized.ResourceNotFound
            ? (lang == "uk"
                ? "Рекомендується скоригувати: калорії {0} ккал, білки {1} г, жири {2} г, вуглеводи {3} г"
                : "Suggested adjustment: calories {0} kcal, protein {1} g, fat {2} g, carbs {3} g")
            : localized.Value;

        var protein = FormatWithUnits(deltas.ProteinDelta, units, isWeight: true);
        var fat = FormatWithUnits(deltas.FatDelta, units, isWeight: true);
        var carbs = FormatWithUnits(deltas.CarbsDelta, units, isWeight: true);
        var calories = FormatDelta(deltas.CaloriesDelta);

        return string.Format(
            templateValue,
            calories,
            protein,
            fat,
            carbs);
    }

    private static MacroNutrientsDto ExtractCurrentMacros(DailyDietPlan plan)
    {
        return new MacroNutrientsDto
        {
            Calories = plan.DailyPlanCalories,
            ProteinGrams = plan.DailyPlanProtein,
            FatGrams = plan.DailyPlanFat,
            CarbsGrams = plan.DailyPlanCarbs
        };
    }

    private static bool HasMeaningfulChange(DailyDietPlan plan, MacroNutrientsDto corrected)
    {
        const decimal epsilon = 0.01m;
        return Math.Abs(plan.DailyPlanCalories - corrected.Calories) > epsilon
               || Math.Abs(plan.DailyPlanProtein - corrected.ProteinGrams) > epsilon
               || Math.Abs(plan.DailyPlanFat - corrected.FatGrams) > epsilon
               || Math.Abs(plan.DailyPlanCarbs - corrected.CarbsGrams) > epsilon;
    }

    private async Task<string> ResolveLocale(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user?.Locale ?? "en";
    }

    private static string GetLanguage(string locale)
    {
        if (string.IsNullOrWhiteSpace(locale))
        {
            return "en";
        }

        var lower = locale.ToLowerInvariant();
        if (lower.StartsWith("uk") || lower.StartsWith("ua"))
        {
            return "uk";
        }

        return "en";
    }

    private string BuildReason(ActivityChangeResult activity, SleepQualityAnalysis sleep, string locale)
    {
        var lang = GetLanguage(locale);
        var reasons = new List<string>();
        if (activity.StepsSpike)
        {
            reasons.Add(_localizer["Recommendations.DietCorrection.Reason.HighActivity"]);
        }
        else if (activity.StepsChangePercent.HasValue && activity.StepsChangePercent.Value < -0.30m)
        {
            reasons.Add(_localizer["Recommendations.DietCorrection.Reason.LowActivity"]);
        }

        if (sleep.IsSleepDeprived)
        {
            reasons.Add(_localizer["Recommendations.DietCorrection.Reason.SleepDeprived"]);
        }

        if (activity.HeartRateAnomaly)
        {
            reasons.Add(_localizer["Recommendations.DietCorrection.Reason.HeartRateAnomaly"]);
        }

        return reasons.Any()
            ? string.Join("; ", reasons)
            : _localizer["Recommendations.DietCorrection.Reason.Default"];
    }

    private static (decimal CaloriesDelta, decimal ProteinDelta, decimal FatDelta, decimal CarbsDelta) BuildDeltas(
        DailyDietPlan plan,
        MacroNutrientsDto target)
    {
        return (
            CaloriesDelta: target.Calories - plan.DailyPlanCalories,
            ProteinDelta: target.ProteinGrams - plan.DailyPlanProtein,
            FatDelta: target.FatGrams - plan.DailyPlanFat,
            CarbsDelta: target.CarbsGrams - plan.DailyPlanCarbs
        );
    }

    private static string FormatDelta(decimal delta)
    {
        if (delta == 0) return "0";
        return delta > 0 ? $"+{Math.Round(delta, 1)}" : $"{Math.Round(delta, 1)}";
    }

    private string FormatWithUnits(decimal deltaGrams, PreferredUnits units, bool isWeight)
    {
        if (!isWeight)
        {
            return FormatDelta(deltaGrams);
        }

        decimal value = deltaGrams;
        string unitLabel = "g";

        if (units == PreferredUnits.Imperial)
        {
            value = _unitConversionService.ConvertWeight(deltaGrams, PreferredUnits.Metric, PreferredUnits.Imperial, 2);
            unitLabel = "oz";
        }

        var formatted = value == 0 ? "0" : (value > 0 ? $"+{value}" : $"{value}");
        return $"{formatted} {unitLabel}";
    }

    public async Task RebalanceMealsAsync(DailyDietPlan plan, MacroNutrientsDto oldTotals, MacroNutrientsDto newTotals)
    {
        if (oldTotals.Calories <= 0 || newTotals.Calories <= 0)
        {
            return;
        }

        var ratioCalories = newTotals.Calories / oldTotals.Calories;
        var ratioProtein = oldTotals.ProteinGrams > 0 ? newTotals.ProteinGrams / oldTotals.ProteinGrams : (decimal?)null;
        var ratioFat = oldTotals.FatGrams > 0 ? newTotals.FatGrams / oldTotals.FatGrams : (decimal?)null;
        var ratioCarbs = oldTotals.CarbsGrams > 0 ? newTotals.CarbsGrams / oldTotals.CarbsGrams : (decimal?)null;

        var meals = await _mealRepository.FindAsync(m => m.DailyDietPlanId == plan.DailyDietPlanId);
        foreach (var meal in meals)
        {
            var protein = ratioProtein.HasValue ? meal.MealTargetProtein * ratioProtein.Value : meal.MealTargetProtein;
            var fat = ratioFat.HasValue ? meal.MealTargetFat * ratioFat.Value : meal.MealTargetFat;
            var carbs = ratioCarbs.HasValue ? meal.MealTargetCarbs * ratioCarbs.Value : meal.MealTargetCarbs;
            var calories = protein * 4m + fat * 9m + carbs * 4m;

            meal.MealTargetProtein = Math.Round(protein, 1);
            meal.MealTargetFat = Math.Round(fat, 1);
            meal.MealTargetCarbs = Math.Round(carbs, 1);
            meal.MealTargetCalories = Math.Round(calories, 1);
            await _mealRepository.UpdateAsync(meal);
            
            var mealDetails = await _mealRepository.GetMealDetailsByIdAsync(meal.MealId);
            if (mealDetails?.MealRecipes == null || !mealDetails.MealRecipes.Any())
            {
                continue;
            }

            var recipesCount = mealDetails.MealRecipes.Count;
            foreach (var mealRecipe in mealDetails.MealRecipes)
            {
                var recipeDetails = await _recipeRepository.GetRecipeDetailsByIdAsync(mealRecipe.RecipeId);
                var perRecipeTargetCalories = recipesCount > 0
                    ? meal.MealTargetCalories / recipesCount
                    : meal.MealTargetCalories;

                if (recipeDetails != null)
                {
                    var portionMultiplier = PortionCalculator.CalculatePortionMultiplier(recipeDetails, perRecipeTargetCalories);
                    var portionsMetadata = PortionCalculator.BuildPortionsMetadata(recipeDetails, portionMultiplier);
                    mealRecipe.PortionsMetadata = portionsMetadata;
                }
                else
                {
                    mealRecipe.PortionsMetadata = string.Empty;
                }

                await _mealRecipeRepository.UpdateAsync(mealRecipe);
            }
        }
    }
}

