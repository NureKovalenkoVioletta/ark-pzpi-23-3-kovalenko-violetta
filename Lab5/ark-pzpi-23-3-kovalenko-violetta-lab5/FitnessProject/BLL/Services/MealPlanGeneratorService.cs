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
        
        // Логування для діагностики
        System.Diagnostics.Debug.WriteLine($"[MealPlanGenerator] UserId: {userId}, Restrictions: {profile.MedicalConditions}");
        System.Diagnostics.Debug.WriteLine($"[MealPlanGenerator] Total products: {allProducts.Count()}, Allowed products: {allowedProducts.Count}");
        System.Diagnostics.Debug.WriteLine($"[MealPlanGenerator] Allowed product IDs: {string.Join(", ", allowedProductIds.Take(10))}...");

        var recentlyUsedRecipeIds = await GetRecentlyUsedRecipes(userId, date, RECENT_DAYS_TO_EXCLUDE);

        var mealsByTime = new Dictionary<MealTime, List<Recipe>>();
        var recipeUsageCount = new Dictionary<int, int>();

        foreach (var mealTime in new[] { MealTime.Breakfast, MealTime.Lunch, MealTime.Dinner, MealTime.Snack })
        {
            var mealCalories = caloriesByMealTime[mealTime];
            var mealMacros = macrosByMealTime[mealTime];
            
            System.Diagnostics.Debug.WriteLine($"[MealPlanGenerator] Вибір рецептів для {mealTime}: цільові калорії={mealCalories:F1}, білки={mealMacros.ProteinGrams:F1}г");
            
            var recipes = await SelectRecipesForMeal(
                mealCalories, 
                mealMacros, 
                allowedProductIds, 
                recentlyUsedRecipeIds,
                recipeUsageCount);
            
            System.Diagnostics.Debug.WriteLine($"[MealPlanGenerator] Для {mealTime} знайдено {recipes.Count} рецептів: {string.Join(", ", recipes.Select(r => r.RecipeName))}");
            
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
            
            // Якщо у рецепта немає продуктів, вважаємо його дозволеним (може бути простий рецепт)
            if (recipeDetails?.RecipeProducts == null || !recipeDetails.RecipeProducts.Any())
            {
                recipesWithProducts.Add((recipe, true));
                System.Diagnostics.Debug.WriteLine($"[MealPlanGenerator] Recipe '{recipe.RecipeName}' не має продуктів, вважається дозволеним");
                continue;
            }
            
            var hasAllowedProducts = recipeDetails.RecipeProducts
                .All(rp => allowedProductIds.Contains(rp.ProductId));
            
            if (!hasAllowedProducts)
            {
                var blockedProducts = recipeDetails.RecipeProducts
                    .Where(rp => !allowedProductIds.Contains(rp.ProductId))
                    .Select(rp => rp.ProductId)
                    .ToList();
                System.Diagnostics.Debug.WriteLine($"[MealPlanGenerator] Recipe '{recipe.RecipeName}' blocked. Blocked product IDs: {string.Join(", ", blockedProducts)}");
            }
            
            recipesWithProducts.Add((recipe, hasAllowedProducts));
        }

        var filteredRecipes = recipesWithProducts
            .Where(r => r.hasAllowedProducts && CanUseRecipe(r.recipe, recipeUsageCount))
            .Select(r => r.recipe)
            .ToList();
        
        System.Diagnostics.Debug.WriteLine($"[MealPlanGenerator] Available recipes: {availableRecipes.Count}, Filtered recipes: {filteredRecipes.Count}");

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

        // Fallback: якщо не знайдено ідеальних рецептів, беремо найближчі
        if (selectedRecipes.Count == 0)
        {
            System.Diagnostics.Debug.WriteLine($"[MealPlanGenerator] Не знайдено ідеальних рецептів. Filtered: {filteredRecipes.Count}, Available: {availableRecipes.Count}");
            
            if (filteredRecipes.Any())
            {
                // Беремо найближчий рецепт за калоріями
                var bestAllowed = sortedRecipes.FirstOrDefault(r => CanUseRecipe(r, recipeUsageCount));
                var bestRecipe = bestAllowed ?? sortedRecipes.First();

                if (bestRecipe != null)
                {
                    selectedRecipes.Add(bestRecipe);
                    IncrementUsage(bestRecipe, recipeUsageCount);
                    System.Diagnostics.Debug.WriteLine($"[MealPlanGenerator] Fallback (filtered): використано рецепт '{bestRecipe.RecipeName}' ({bestRecipe.RecipeCaloriesPerPortion} ккал) для прийому з ціллю {targetCalories} ккал");
                }
            }
            else if (availableRecipes.Any())
            {
                // Якщо всі рецепти відфільтровані, беремо найближчий доступний (ігноруючи фільтри продуктів)
                var fallbackRecipe = availableRecipes
                    .OrderBy(r => Math.Abs(r.RecipeCaloriesPerPortion - targetCalories))
                    .FirstOrDefault(r => CanUseRecipe(r, recipeUsageCount));
                
                if (fallbackRecipe != null)
                {
                    selectedRecipes.Add(fallbackRecipe);
                    IncrementUsage(fallbackRecipe, recipeUsageCount);
                    System.Diagnostics.Debug.WriteLine($"[MealPlanGenerator] Fallback (без фільтрів продуктів): використано рецепт '{fallbackRecipe.RecipeName}' ({fallbackRecipe.RecipeCaloriesPerPortion} ккал) для прийому з ціллю {targetCalories} ккал");
                }
                else
                {
                    // Останній fallback - беремо будь-який доступний рецепт
                    var anyRecipe = availableRecipes
                        .OrderBy(r => Math.Abs(r.RecipeCaloriesPerPortion - targetCalories))
                        .FirstOrDefault();
                    
                    if (anyRecipe != null)
                    {
                        selectedRecipes.Add(anyRecipe);
                        IncrementUsage(anyRecipe, recipeUsageCount);
                        System.Diagnostics.Debug.WriteLine($"[MealPlanGenerator] Fallback (останній варіант): використано рецепт '{anyRecipe.RecipeName}' ({anyRecipe.RecipeCaloriesPerPortion} ккал) для прийому з ціллю {targetCalories} ккал");
                    }
                }
            }
            
            if (selectedRecipes.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine($"[MealPlanGenerator] КРИТИЧНО: Не знайдено жодного рецепта для прийому! Available recipes: {availableRecipes.Count}");
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

        // Перевіряємо, що всі 4 типи прийомів присутні
        var allMealTimes = new[] { MealTime.Breakfast, MealTime.Lunch, MealTime.Dinner, MealTime.Snack };
        foreach (var mealTime in allMealTimes)
        {
            if (!mealsByTime.ContainsKey(mealTime))
            {
                mealsByTime[mealTime] = new List<Recipe>();
                System.Diagnostics.Debug.WriteLine($"[MealPlanGenerator] Додано порожній список рецептів для {mealTime}");
            }
        }
        
        foreach (var (mealTime, recipes) in mealsByTime.OrderBy(m => m.Key))
        {
            // Створюємо прийом навіть якщо рецептів немає (з цільовими значеннями)
            // Це забезпечує, що всі 4 прийоми (Breakfast, Lunch, Dinner, Snack) завжди створюються
            if (!recipes.Any())
            {
                System.Diagnostics.Debug.WriteLine($"[MealPlanGenerator] Попередження: для {mealTime} не знайдено рецептів, створюється прийом з цільовими значеннями");
                
                // Створюємо прийом з цільовими значеннями з macrosByMealTime
                var mealMacros = macrosByMealTime[mealTime];
                var emptyMeal = new Meal
                {
                    DailyDietPlanId = plan.DailyDietPlanId,
                    MealTime = mealTime,
                    MealOrder = mealOrder++,
                    MealTargetCalories = Math.Round(mealMacros.Calories, 1),
                    MealTargetProtein = Math.Round(mealMacros.ProteinGrams, 1),
                    MealTargetFat = Math.Round(mealMacros.FatGrams, 1),
                    MealTargetCarbs = Math.Round(mealMacros.CarbsGrams, 1)
                };

                var savedEmptyMeal = await _mealRepository.AddAsync(emptyMeal);
                System.Diagnostics.Debug.WriteLine($"[MealPlanGenerator] Створено прийом {mealTime} (ID={savedEmptyMeal.MealId}) без рецептів з цільовими значеннями");
                continue;
            }

            var mealMacrosForMeal = macrosByMealTime[mealTime];
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
                var perRecipeTargetCalories = mealMacrosForMeal.Calories / recipes.Count;
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
        
        // Фінальна перевірка: скільки прийомів створено
        var allMealsForPlan = await _mealRepository.FindAsync(m => m.DailyDietPlanId == plan.DailyDietPlanId);
        var mealsList = allMealsForPlan.ToList();
        System.Diagnostics.Debug.WriteLine($"[MealPlanGenerator] ВСЬОГО створено прийомів для плану {plan.DailyDietPlanId}: {mealsList.Count}");
        foreach (var m in mealsList.OrderBy(m => m.MealTime))
        {
            System.Diagnostics.Debug.WriteLine($"[MealPlanGenerator]   - {m.MealTime} (ID={m.MealId}, Order={m.MealOrder}, Calories={m.MealTargetCalories})");
        }
    }
}

