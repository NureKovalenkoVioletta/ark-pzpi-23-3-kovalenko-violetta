using FitnessProject.BLL.DTO.MacroNutrients;
using FitnessProject.BLL.DTO.MedicalRestrictions;
using FitnessProject.BLL.Services.Helpers;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;
using FitnessProject.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FitnessProject.BLL.Services;

public class MealPlanGeneratorService : IMealPlanGeneratorService
{
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IRecipeRepository _recipeRepository;
    private readonly IProductRepository _productRepository;
    private readonly IDailyDietPlanRepository _dailyDietPlanRepository;
    private readonly IMealRepository _mealRepository;
    private readonly IMealRecipeRepository _mealRecipeRepository;

    private const decimal BREAKFAST_CALORIE_PERCENTAGE = 0.275m;
    private const decimal LUNCH_CALORIE_PERCENTAGE = 0.325m;
    private const decimal DINNER_CALORIE_PERCENTAGE = 0.275m;
    private const decimal SNACK_CALORIE_PERCENTAGE = 0.125m;

    private const int RECENT_DAYS_TO_EXCLUDE = 7;
    private const decimal CALORIE_TOLERANCE = 0.15m;
    private const decimal MACRO_TOLERANCE = 0.20m;
    private const int MAX_RECIPE_USAGE_PER_DAY = 1;

    public MealPlanGeneratorService(
        IUserProfileRepository userProfileRepository,
        IRecipeRepository recipeRepository,
        IProductRepository productRepository,
        IDailyDietPlanRepository dailyDietPlanRepository,
        IMealRepository mealRepository,
        IMealRecipeRepository mealRecipeRepository)
    {
        _userProfileRepository = userProfileRepository;
        _recipeRepository = recipeRepository;
        _productRepository = productRepository;
        _dailyDietPlanRepository = dailyDietPlanRepository;
        _mealRepository = mealRepository;
        _mealRecipeRepository = mealRecipeRepository;
    }

    public async Task<DailyDietPlan> GenerateMealPlanAsync(int userId, DateTime date, int? templateDietPlanId = null)
    {
        var userProfile = await _userProfileRepository.FindAsync(up => up.UserId == userId);
        var profile = userProfile.FirstOrDefault();
        
        if (profile == null)
        {
            throw new ArgumentException($"UserProfile for UserId {userId} not found");
        }

        var age = CalorieCalculator.CalculateAge(profile.BirthDate);
        var bmr = CalorieCalculator.CalculateBMR(profile.CurrentWeightKg, profile.HeightCm, age, profile.Sex);
        var tdee = CalorieCalculator.CalculateTDEE(bmr, profile.ActivityLevel);
        var targetCalories = MacroNutrientsCalculator.CalculateCaloriesByGoal(tdee, profile.GoalType);
        var dailyMacros = MacroNutrientsCalculator.CalculateMacros(targetCalories, profile.CurrentWeightKg, profile.GoalType);

        var caloriesByMealTime = DistributeCaloriesByMealTime(dailyMacros.Calories);
        var macrosByMealTime = BalanceMacrosForMeals(caloriesByMealTime, dailyMacros);

        var restrictions = MedicalRestrictionsParser.ParseMedicalConditions(profile.MedicalConditions);
        var allProducts = await _productRepository.GetAllAsync();
        var allowedProducts = ProductFilterHelper.FilterProductsByRestrictions(allProducts, restrictions).ToList();
        var allowedProductIds = allowedProducts.Select(p => p.ProductId).ToHashSet();

        var recentlyUsedRecipeIds = await GetRecentlyUsedRecipes(userId, date, RECENT_DAYS_TO_EXCLUDE);

        var mealsByTime = new Dictionary<MealTime, List<Recipe>>();
        var recipeUsageCount = new Dictionary<int, int>();

        foreach (var mealTime in new[] { MealTime.Breakfast, MealTime.Lunch, MealTime.Dinner, MealTime.Snack })
        {
            var mealCalories = caloriesByMealTime[mealTime];
            var mealMacros = macrosByMealTime[mealTime];
            
            var recipes = await SelectRecipesForMeal(
                mealCalories, 
                mealMacros, 
                allowedProductIds, 
                recentlyUsedRecipeIds,
                recipeUsageCount);
            
            mealsByTime[mealTime] = recipes;
        }

        var dailyDietPlan = new DailyDietPlan
        {
            UserId = userId,
            DailyDietPlanName = $"Meal Plan for {date:yyyy-MM-dd}",
            DailyPlanCalories = dailyMacros.Calories,
            DailyPlanFat = dailyMacros.FatGrams,
            DailyPlanCarbs = dailyMacros.CarbsGrams,
            DailyPlanProtein = dailyMacros.ProteinGrams,
            DailyPlanNumberOfMeals = mealsByTime.Values.Sum(r => r.Count),
            DailyPlanStatus = DailyPlanStatus.Planned,
            DailyPlanCreatedAt = DateTime.UtcNow
        };

        var savedPlan = await _dailyDietPlanRepository.AddAsync(dailyDietPlan);

        await CreateMealsForPlanAsync(savedPlan, mealsByTime, macrosByMealTime);

        return savedPlan;
    }

    private Dictionary<MealTime, decimal> DistributeCaloriesByMealTime(decimal totalCalories)
    {
        return new Dictionary<MealTime, decimal>
        {
            { MealTime.Breakfast, Math.Round(totalCalories * BREAKFAST_CALORIE_PERCENTAGE, 1) },
            { MealTime.Lunch, Math.Round(totalCalories * LUNCH_CALORIE_PERCENTAGE, 1) },
            { MealTime.Dinner, Math.Round(totalCalories * DINNER_CALORIE_PERCENTAGE, 1) },
            { MealTime.Snack, Math.Round(totalCalories * SNACK_CALORIE_PERCENTAGE, 1) }
        };
    }

    private Dictionary<MealTime, MacroNutrientsDto> BalanceMacrosForMeals(
        Dictionary<MealTime, decimal> caloriesByMealTime, 
        MacroNutrientsDto dailyMacros)
    {
        var macrosByMealTime = new Dictionary<MealTime, MacroNutrientsDto>();

        foreach (var (mealTime, mealCalories) in caloriesByMealTime)
        {
            var calorieRatio = mealCalories / dailyMacros.Calories;
            
            macrosByMealTime[mealTime] = new MacroNutrientsDto
            {
                Calories = mealCalories,
                ProteinGrams = Math.Round(dailyMacros.ProteinGrams * calorieRatio, 1),
                FatGrams = Math.Round(dailyMacros.FatGrams * calorieRatio, 1),
                CarbsGrams = Math.Round(dailyMacros.CarbsGrams * calorieRatio, 1)
            };
        }

        return macrosByMealTime;
    }

    private async Task<List<int>> GetRecentlyUsedRecipes(int userId, DateTime date, int days)
    {
        var startDate = date.AddDays(-days);
        
        var recentPlans = await _dailyDietPlanRepository.FindAsync(ddp => 
            ddp.UserId == userId && 
            ddp.DailyPlanCreatedAt >= startDate && 
            ddp.DailyPlanCreatedAt < date);

        var recipeIds = new HashSet<int>();

        foreach (var plan in recentPlans)
        {
            var planDetails = await _dailyDietPlanRepository.GetDailyDietPlanDetailsByIdAsync(plan.DailyDietPlanId);
            if (planDetails?.Meals != null)
            {
                foreach (var meal in planDetails.Meals)
                {
                    var mealDetails = await _mealRepository.GetMealDetailsByIdAsync(meal.MealId);
                    if (mealDetails?.MealRecipes != null)
                    {
                        foreach (var mealRecipe in mealDetails.MealRecipes)
                        {
                            recipeIds.Add(mealRecipe.RecipeId);
                        }
                    }
                }
            }
        }

        return recipeIds.ToList();
    }

    private async Task<List<Recipe>> SelectRecipesForMeal(
        decimal targetCalories,
        MacroNutrientsDto targetMacros,
        HashSet<int> allowedProductIds,
        List<int> excludedRecipeIds,
        Dictionary<int, int> recipeUsageCount)
    {
        var allRecipes = await _recipeRepository.GetAllAsync();
        
        var availableRecipes = allRecipes
            .Where(r => !excludedRecipeIds.Contains(r.RecipeId))
            .ToList();

        var recipesWithProducts = new List<(Recipe recipe, bool hasAllowedProducts)>();

        foreach (var recipe in availableRecipes)
        {
            var recipeDetails = await _recipeRepository.GetRecipeDetailsByIdAsync(recipe.RecipeId);
            if (recipeDetails?.RecipeProducts != null)
            {
                var hasAllowedProducts = recipeDetails.RecipeProducts
                    .All(rp => allowedProductIds.Contains(rp.ProductId));
                
                recipesWithProducts.Add((recipe, hasAllowedProducts));
            }
        }

        var filteredRecipes = recipesWithProducts
            .Where(r => r.hasAllowedProducts && CanUseRecipe(r.recipe, recipeUsageCount))
            .Select(r => r.recipe)
            .ToList();

        var selectedRecipes = new List<Recipe>();
        var currentCalories = 0m;
        var currentProtein = 0m;
        var currentFat = 0m;
        var currentCarbs = 0m;

        var sortedRecipes = filteredRecipes
            .OrderBy(r => Math.Abs(r.RecipeCaloriesPerPortion - targetCalories))
            .ToList();

        foreach (var recipe in sortedRecipes)
        {
            if (!CanUseRecipe(recipe, recipeUsageCount))
            {
                continue;
            }

            var newCalories = currentCalories + recipe.RecipeCaloriesPerPortion;
            var newProtein = currentProtein + recipe.RecipeProteinPerPortion;
            var newFat = currentFat + recipe.RecipeFatPerPortion;
            var newCarbs = currentCarbs + recipe.RecipeCarbsPerPortion;

            if (newCalories <= targetCalories * (1 + CALORIE_TOLERANCE))
            {
                var caloriesDiff = Math.Abs(newCalories - targetCalories) / targetCalories;
                var proteinDiff = Math.Abs(newProtein - targetMacros.ProteinGrams) / targetMacros.ProteinGrams;
                var fatDiff = Math.Abs(newFat - targetMacros.FatGrams) / targetMacros.FatGrams;
                var carbsDiff = Math.Abs(newCarbs - targetMacros.CarbsGrams) / targetMacros.CarbsGrams;

                if (caloriesDiff <= CALORIE_TOLERANCE && 
                    proteinDiff <= MACRO_TOLERANCE && 
                    fatDiff <= MACRO_TOLERANCE && 
                    carbsDiff <= MACRO_TOLERANCE)
                {
                    selectedRecipes.Add(recipe);
                    IncrementUsage(recipe, recipeUsageCount);
                    currentCalories = newCalories;
                    currentProtein = newProtein;
                    currentFat = newFat;
                    currentCarbs = newCarbs;

                    if (currentCalories >= targetCalories * (1 - CALORIE_TOLERANCE))
                    {
                        break;
                    }
                }
            }
        }

        if (selectedRecipes.Count == 0 && filteredRecipes.Any())
        {
            var bestAllowed = sortedRecipes.FirstOrDefault(r => CanUseRecipe(r, recipeUsageCount));
            var bestRecipe = bestAllowed ?? sortedRecipes.First();

            if (CanUseRecipe(bestRecipe, recipeUsageCount))
            {
                selectedRecipes.Add(bestRecipe);
                IncrementUsage(bestRecipe, recipeUsageCount);
            }
        }

        return selectedRecipes;
    }

    private static bool CanUseRecipe(Recipe recipe, Dictionary<int, int> usage)
    {
        return !usage.TryGetValue(recipe.RecipeId, out var count) || count < MAX_RECIPE_USAGE_PER_DAY;
    }

    private static void IncrementUsage(Recipe recipe, Dictionary<int, int> usage)
    {
        if (usage.TryGetValue(recipe.RecipeId, out var count))
        {
            usage[recipe.RecipeId] = count + 1;
        }
        else
        {
            usage[recipe.RecipeId] = 1;
        }
    }

    private async Task CreateMealsForPlanAsync(
        DailyDietPlan plan,
        Dictionary<MealTime, List<Recipe>> mealsByTime,
        Dictionary<MealTime, MacroNutrientsDto> macrosByMealTime)
    {
        var mealOrder = 1;

        foreach (var (mealTime, recipes) in mealsByTime.OrderBy(m => m.Key))
        {
            if (!recipes.Any())
            {
                continue;
            }

            var mealMacros = macrosByMealTime[mealTime];
            var totalCalories = recipes.Sum(r => r.RecipeCaloriesPerPortion);
            var totalProtein = recipes.Sum(r => r.RecipeProteinPerPortion);
            var totalFat = recipes.Sum(r => r.RecipeFatPerPortion);
            var totalCarbs = recipes.Sum(r => r.RecipeCarbsPerPortion);

            var meal = new Meal
            {
                DailyDietPlanId = plan.DailyDietPlanId,
                MealTime = mealTime,
                MealOrder = mealOrder++,
                MealTargetCalories = Math.Round(totalCalories, 1),
                MealTargetProtein = Math.Round(totalProtein, 1),
                MealTargetFat = Math.Round(totalFat, 1),
                MealTargetCarbs = Math.Round(totalCarbs, 1)
            };

            var savedMeal = await _mealRepository.AddAsync(meal);

            foreach (var recipe in recipes)
            {
                var recipeDetails = await _recipeRepository.GetRecipeDetailsByIdAsync(recipe.RecipeId);
                var perRecipeTargetCalories = mealMacros.Calories / recipes.Count;
                var portionMultiplier = PortionCalculator.CalculatePortionMultiplier(recipe, perRecipeTargetCalories);
                var portionsMetadata = recipeDetails != null
                    ? PortionCalculator.BuildPortionsMetadata(recipeDetails, portionMultiplier)
                    : string.Empty;

                var mealRecipe = new MealRecipe
                {
                    MealId = savedMeal.MealId,
                    RecipeId = recipe.RecipeId,
                    PortionsMetadata = portionsMetadata
                };

                await _mealRecipeRepository.AddAsync(mealRecipe);
            }
        }
    }
}

