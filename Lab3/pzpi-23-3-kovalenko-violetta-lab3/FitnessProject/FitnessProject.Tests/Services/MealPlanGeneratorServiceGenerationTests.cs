using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using FitnessProject.BLL.DTO.MedicalRestrictions;
using FitnessProject.BLL.Services;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;
using FitnessProject.Enums;
using Moq;
using Xunit;

namespace FitnessProject.Tests.Services;

public class MealPlanGeneratorServiceGenerationTests
{
    private MealPlanGeneratorService CreateService(
        out Mock<IUserProfileRepository> userProfiles,
        out Mock<IRecipeRepository> recipes,
        out Mock<IProductRepository> products,
        out Mock<IDailyDietPlanRepository> plans,
        out Mock<IMealRepository> meals,
        out Mock<IMealRecipeRepository> mealRecipes)
    {
        userProfiles = new Mock<IUserProfileRepository>();
        recipes = new Mock<IRecipeRepository>();
        products = new Mock<IProductRepository>();
        plans = new Mock<IDailyDietPlanRepository>();
        meals = new Mock<IMealRepository>();
        mealRecipes = new Mock<IMealRecipeRepository>();

        return new MealPlanGeneratorService(
            userProfiles.Object,
            recipes.Object,
            products.Object,
            plans.Object,
            meals.Object,
            mealRecipes.Object);
    }

    [Fact]
    public async Task GenerateMealPlan_HappyPath_CreatesPlanAndMeals()
    {
        var svc = CreateService(
            out var userProfiles,
            out var recipes,
            out var products,
            out var plans,
            out var meals,
            out var mealRecipes);

        var profile = new UserProfile
        {
            UserId = 1,
            BirthDate = DateTime.Today.AddYears(-30),
            CurrentWeightKg = 80,
            HeightCm = 180,
            Sex = Sex.Male,
            ActivityLevel = ActivityLevel.Sedentary,
            GoalType = GoalType.WeightMaintenance,
            MedicalConditions = "{}"
        };
        userProfiles.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserProfile, bool>>>()))
            .ReturnsAsync(new List<UserProfile> { profile });

        var productList = new List<Product>
        {
            new() { ProductId = 1, ProductName = "Tofu", Tags = ProductTags.None }
        };
        products.Setup(r => r.GetAllAsync()).ReturnsAsync(productList);

        var recipeList = new List<Recipe>
        {
            Recipe(1, 550, 30, 20, 60),
            Recipe(2, 650, 35, 25, 75),
            Recipe(3, 550, 30, 20, 60),
            Recipe(4, 250, 15, 10, 25)
        };
        recipes.Setup(r => r.GetAllAsync()).ReturnsAsync(recipeList);
        recipes.Setup(r => r.GetRecipeDetailsByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => new Recipe
            {
                RecipeId = id,
                RecipeProducts = new List<RecipeProduct>
                {
                    new() { ProductId = 1 }
                }
            });

        plans.Setup(p => p.AddAsync(It.IsAny<DailyDietPlan>()))
            .ReturnsAsync((DailyDietPlan plan) =>
            {
                plan.DailyDietPlanId = 1;
                return plan;
            });

        var mealId = 1;
        meals.Setup(m => m.AddAsync(It.IsAny<Meal>()))
            .ReturnsAsync((Meal meal) =>
            {
                meal.MealId = mealId++;
                return meal;
            });

        mealRecipes.Setup(m => m.AddAsync(It.IsAny<MealRecipe>()))
            .ReturnsAsync((MealRecipe mr) => mr);

        plans.Setup(p => p.FindAsync(It.IsAny<Expression<Func<DailyDietPlan, bool>>>()))
            .ReturnsAsync(new List<DailyDietPlan>());

        meals.Setup(m => m.GetMealDetailsByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Meal?)null);

        plans.Setup(p => p.GetDailyDietPlanDetailsByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((DailyDietPlan?)null);

        var result = await svc.GenerateMealPlanAsync(1, DateTime.Today);

        Assert.NotNull(result);
        Assert.Equal(1, result.UserId);
        Assert.Equal(4, result.DailyPlanNumberOfMeals);

        plans.Verify(p => p.AddAsync(It.IsAny<DailyDietPlan>()), Times.Once);
        meals.Verify(m => m.AddAsync(It.IsAny<Meal>()), Times.Exactly(4));
        mealRecipes.Verify(mr => mr.AddAsync(It.IsAny<MealRecipe>()), Times.Exactly(4));
    }

    [Fact]
    public async Task GenerateMealPlan_EmptyRecipesOrProducts_DoesNotThrow()
    {
        var svc = CreateService(
            out var userProfiles,
            out var recipes,
            out var products,
            out var plans,
            out var meals,
            out var mealRecipes);

        var profile = new UserProfile
        {
            UserId = 2,
            BirthDate = DateTime.Today.AddYears(-25),
            CurrentWeightKg = 60,
            HeightCm = 165,
            Sex = Sex.Female,
            ActivityLevel = ActivityLevel.Sedentary,
            GoalType = GoalType.WeightMaintenance,
            MedicalConditions = "{}"
        };

        userProfiles.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserProfile, bool>>>()))
            .ReturnsAsync(new List<UserProfile> { profile });

        products.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product>());
        recipes.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Recipe>());
        recipes.Setup(r => r.GetRecipeDetailsByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Recipe?)null);

        plans.Setup(p => p.AddAsync(It.IsAny<DailyDietPlan>()))
            .ReturnsAsync((DailyDietPlan plan) =>
            {
                plan.DailyDietPlanId = 10;
                return plan;
            });

        plans.Setup(p => p.FindAsync(It.IsAny<Expression<Func<DailyDietPlan, bool>>>()))
            .ReturnsAsync(new List<DailyDietPlan>());
        meals.Setup(m => m.GetMealDetailsByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Meal?)null);
        plans.Setup(p => p.GetDailyDietPlanDetailsByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((DailyDietPlan?)null);

        var result = await svc.GenerateMealPlanAsync(2, DateTime.Today);

        Assert.NotNull(result);
        Assert.Equal(2, result.UserId);
        Assert.Equal(0, result.DailyPlanNumberOfMeals);
    }

    [Fact]
    public async Task GenerateMealPlan_VeganDiabetes_Allergens_DisallowedRecipesExcluded()
    {
        var svc = CreateService(
            out var userProfiles,
            out var recipes,
            out var products,
            out var plans,
            out var meals,
            out var mealRecipes);

        var profile = new UserProfile
        {
            UserId = 3,
            BirthDate = DateTime.Today.AddYears(-28),
            CurrentWeightKg = 70,
            HeightCm = 175,
            Sex = Sex.Female,
            ActivityLevel = ActivityLevel.Sedentary,
            GoalType = GoalType.WeightMaintenance,
            MedicalConditions = """{ "medicalConditions":["Diabetes"], "dietaryRestriction":"Vegan", "allergens":["Eggs"] }"""
        };

        userProfiles.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserProfile, bool>>>()))
            .ReturnsAsync(new List<UserProfile> { profile });

        var productList = new List<Product>
        {
            new() { ProductId = 1, ProductName = "Chicken", Tags = ProductTags.Meat },
            new() { ProductId = 2, ProductName = "Honey Bar", Tags = ProductTags.Honey | ProductTags.Sugar },
            new() { ProductId = 3, ProductName = "Egg Powder", Tags = ProductTags.Egg, Allergens = "Eggs" },
            new() { ProductId = 4, ProductName = "Tofu", Tags = ProductTags.None }
        };
        products.Setup(r => r.GetAllAsync()).ReturnsAsync(productList);

        var safeRecipes = new List<Recipe>
        {
            Recipe(101, 500, 30, 15, 60),
            Recipe(102, 520, 32, 14, 62),
            Recipe(103, 480, 28, 13, 58),
            Recipe(104, 260, 15, 8, 30)
        };
        var badRecipes = new List<Recipe>
        {
            Recipe(201, 500, 30, 15, 60), 
            Recipe(202, 500, 30, 15, 60), 
            Recipe(203, 500, 30, 15, 60)  
        };
        var allRecipes = safeRecipes.Concat(badRecipes).ToList();
        recipes.Setup(r => r.GetAllAsync()).ReturnsAsync(allRecipes);
        recipes.Setup(r => r.GetRecipeDetailsByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) =>
            {
                int productId = id switch
                {
                    201 => 1,
                    202 => 2,
                    203 => 3,
                    _ => 4
                };
                return new Recipe
                {
                    RecipeId = id,
                    RecipeProducts = new List<RecipeProduct>
                    {
                        new() { ProductId = productId }
                    }
                };
            });

        plans.Setup(p => p.AddAsync(It.IsAny<DailyDietPlan>()))
            .ReturnsAsync((DailyDietPlan plan) =>
            {
                plan.DailyDietPlanId = 30;
                return plan;
            });
        plans.Setup(p => p.FindAsync(It.IsAny<Expression<Func<DailyDietPlan, bool>>>()))
            .ReturnsAsync(new List<DailyDietPlan>());
        plans.Setup(p => p.GetDailyDietPlanDetailsByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((DailyDietPlan?)null);

        meals.Setup(m => m.GetMealDetailsByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Meal?)null);

        var mealId = 1;
        meals.Setup(m => m.AddAsync(It.IsAny<Meal>()))
            .ReturnsAsync((Meal meal) =>
            {
                meal.MealId = mealId++;
                return meal;
            });

        var addedRecipeIds = new List<int>();
        mealRecipes.Setup(mr => mr.AddAsync(It.IsAny<MealRecipe>()))
            .ReturnsAsync((MealRecipe mr) =>
            {
                addedRecipeIds.Add(mr.RecipeId);
                return mr;
            });

        var result = await svc.GenerateMealPlanAsync(3, DateTime.Today);

        Assert.NotNull(result);
        Assert.DoesNotContain(addedRecipeIds, id => id == 201 || id == 202 || id == 203);
        Assert.All(addedRecipeIds, id => safeRecipes.Any(r => r.RecipeId == id));
    }

    [Fact]
    public async Task GenerateMealPlan_RecentRecipesExcluded()
    {
        var svc = CreateService(
            out var userProfiles,
            out var recipes,
            out var products,
            out var plans,
            out var meals,
            out var mealRecipes);

        var profile = new UserProfile
        {
            UserId = 4,
            BirthDate = DateTime.Today.AddYears(-35),
            CurrentWeightKg = 75,
            HeightCm = 178,
            Sex = Sex.Male,
            ActivityLevel = ActivityLevel.Sedentary,
            GoalType = GoalType.WeightMaintenance,
            MedicalConditions = "{}"
        };
        userProfiles.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserProfile, bool>>>()))
            .ReturnsAsync(new List<UserProfile> { profile });

        products.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Product> { new() { ProductId = 1, ProductName = "Tofu", Tags = ProductTags.None } });

        var recipeNew = Recipe(301, 500, 30, 15, 60);
        var recipeOld = Recipe(300, 500, 30, 15, 60);
        recipes.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Recipe> { recipeNew, recipeOld });
        recipes.Setup(r => r.GetRecipeDetailsByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => new Recipe
            {
                RecipeId = id,
                RecipeProducts = new List<RecipeProduct> { new() { ProductId = 1 } }
            });

        plans.Setup(p => p.AddAsync(It.IsAny<DailyDietPlan>()))
            .ReturnsAsync((DailyDietPlan plan) =>
            {
                plan.DailyDietPlanId = 40;
                return plan;
            });

        plans.Setup(p => p.FindAsync(It.IsAny<Expression<Func<DailyDietPlan, bool>>>()))
            .ReturnsAsync(new List<DailyDietPlan>
            {
                new()
                {
                    DailyDietPlanId = 99,
                    UserId = 4,
                    DailyPlanCreatedAt = DateTime.UtcNow.AddDays(-1)
                }
            });

        plans.Setup(p => p.GetDailyDietPlanDetailsByIdAsync(99))
            .ReturnsAsync(new DailyDietPlan
            {
                DailyDietPlanId = 99,
                Meals = new List<Meal>
                {
                    new() { MealId = 500 }
                }
            });

        meals.Setup(m => m.GetMealDetailsByIdAsync(500))
            .ReturnsAsync(new Meal
            {
                MealId = 500,
                MealRecipes = new List<MealRecipe>
                {
                    new() { RecipeId = 300 }
                }
            });

        var mealId = 1;
        meals.Setup(m => m.AddAsync(It.IsAny<Meal>()))
            .ReturnsAsync((Meal meal) =>
            {
                meal.MealId = mealId++;
                return meal;
            });

        var addedRecipeIds = new List<int>();
        mealRecipes.Setup(mr => mr.AddAsync(It.IsAny<MealRecipe>()))
            .ReturnsAsync((MealRecipe mr) =>
            {
                addedRecipeIds.Add(mr.RecipeId);
                return mr;
            });

        var result = await svc.GenerateMealPlanAsync(4, DateTime.Today);

        Assert.NotNull(result);
        Assert.DoesNotContain(300, addedRecipeIds);
        Assert.Contains(301, addedRecipeIds);
    }

    [Fact]
    public async Task GenerateMealPlan_RespectsTolerance_AndFallback()
    {
        var svc = CreateService(
            out var userProfiles,
            out var recipes,
            out var products,
            out var plans,
            out var meals,
            out var mealRecipes);

        var profile = new UserProfile
        {
            UserId = 5,
            BirthDate = DateTime.Today.AddYears(-32),
            CurrentWeightKg = 70,
            HeightCm = 175,
            Sex = Sex.Male,
            ActivityLevel = ActivityLevel.Sedentary,
            GoalType = GoalType.WeightMaintenance,
            MedicalConditions = "{}"
        };
        userProfiles.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserProfile, bool>>>()))
            .ReturnsAsync(new List<UserProfile> { profile });

        products.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Product> { new() { ProductId = 1, ProductName = "Tofu", Tags = ProductTags.None } });

        var nearRecipe = Recipe(401, 300, 18, 8, 32);
        var fallbackRecipe = Recipe(402, 280, 16, 7, 30);
        recipes.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Recipe> { nearRecipe, fallbackRecipe });
        recipes.Setup(r => r.GetRecipeDetailsByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => new Recipe
            {
                RecipeId = id,
                RecipeProducts = new List<RecipeProduct> { new() { ProductId = 1 } }
            });

        plans.Setup(p => p.AddAsync(It.IsAny<DailyDietPlan>()))
            .ReturnsAsync((DailyDietPlan plan) =>
            {
                plan.DailyDietPlanId = 50;
                return plan;
            });
        plans.Setup(p => p.FindAsync(It.IsAny<Expression<Func<DailyDietPlan, bool>>>()))
            .ReturnsAsync(new List<DailyDietPlan>());
        plans.Setup(p => p.GetDailyDietPlanDetailsByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((DailyDietPlan?)null);
        meals.Setup(m => m.GetMealDetailsByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Meal?)null);

        var mealId = 1;
        meals.Setup(m => m.AddAsync(It.IsAny<Meal>()))
            .ReturnsAsync((Meal meal) =>
            {
                meal.MealId = mealId++;
                return meal;
            });

        var addedRecipeIds = new List<int>();
        mealRecipes.Setup(mr => mr.AddAsync(It.IsAny<MealRecipe>()))
            .ReturnsAsync((MealRecipe mr) =>
            {
                addedRecipeIds.Add(mr.RecipeId);
                return mr;
            });

        var result = await svc.GenerateMealPlanAsync(5, DateTime.Today);

        Assert.NotNull(result);
        Assert.All(addedRecipeIds, id => Assert.True(id == 401 || id == 402));
    }

    [Fact]
    public async Task GenerateMealPlan_ResultIntegrity_ChecksAllowedIdsAndStatus()
    {
        var svc = CreateService(
            out var userProfiles,
            out var recipes,
            out var products,
            out var plans,
            out var meals,
            out var mealRecipes);

        var profile = new UserProfile
        {
            UserId = 6,
            BirthDate = DateTime.Today.AddYears(-29),
            CurrentWeightKg = 70,
            HeightCm = 175,
            Sex = Sex.Male,
            ActivityLevel = ActivityLevel.Sedentary,
            GoalType = GoalType.WeightMaintenance,
            MedicalConditions = "{}"
        };
        userProfiles.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserProfile, bool>>>()))
            .ReturnsAsync(new List<UserProfile> { profile });

        var allowedProducts = new List<Product>
        {
            new() { ProductId = 1, ProductName = "Tofu", Tags = ProductTags.None },
            new() { ProductId = 2, ProductName = "Broccoli", Tags = ProductTags.None }
        };
        products.Setup(r => r.GetAllAsync()).ReturnsAsync(allowedProducts);

        var recipesList = new List<Recipe>
        {
            Recipe(501, 600, 36, 18, 72),
            Recipe(502, 600, 36, 18, 72)
        };
        recipes.Setup(r => r.GetAllAsync()).ReturnsAsync(recipesList);
        recipes.Setup(r => r.GetRecipeDetailsByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => new Recipe
            {
                RecipeId = id,
                RecipeProducts = new List<RecipeProduct>
                {
                    new() { ProductId = 1 },
                    new() { ProductId = 2 }
                }
            });

        plans.Setup(p => p.AddAsync(It.IsAny<DailyDietPlan>()))
            .ReturnsAsync((DailyDietPlan plan) =>
            {
                plan.DailyDietPlanId = 60;
                return plan;
            });
        plans.Setup(p => p.FindAsync(It.IsAny<Expression<Func<DailyDietPlan, bool>>>()))
            .ReturnsAsync(new List<DailyDietPlan>());
        plans.Setup(p => p.GetDailyDietPlanDetailsByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((DailyDietPlan?)null);
        meals.Setup(m => m.GetMealDetailsByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Meal?)null);

        var mealId = 1;
        meals.Setup(m => m.AddAsync(It.IsAny<Meal>()))
            .ReturnsAsync((Meal meal) =>
            {
                meal.MealId = mealId++;
                return meal;
            });

        var addedMeals = new List<Meal>();
        meals.Setup(m => m.AddAsync(It.IsAny<Meal>()))
            .ReturnsAsync((Meal meal) =>
            {
                meal.MealId = mealId++;
                addedMeals.Add(meal);
                return meal;
            });

        var addedMealRecipes = new List<MealRecipe>();
        mealRecipes.Setup(mr => mr.AddAsync(It.IsAny<MealRecipe>()))
            .ReturnsAsync((MealRecipe mr) =>
            {
                addedMealRecipes.Add(mr);
                return mr;
            });

        var result = await svc.GenerateMealPlanAsync(6, DateTime.Today);

        Assert.NotNull(result);
        Assert.Equal(DailyPlanStatus.Planned, result.DailyPlanStatus);
        Assert.False(string.IsNullOrWhiteSpace(result.DailyDietPlanName));
        Assert.True(result.DailyPlanCreatedAt > DateTime.MinValue);

        var allowedIds = allowedProducts.Select(p => p.ProductId).ToHashSet();
        foreach (var mealRecipe in addedMealRecipes)
        {
            var recipeDetails = await recipes.Object.GetRecipeDetailsByIdAsync(mealRecipe.RecipeId);
            Assert.NotNull(recipeDetails);
            Assert.All(recipeDetails!.RecipeProducts, rp => Assert.Contains(rp.ProductId, allowedIds));
        }

        var totalMealCalories = addedMeals.Sum(m => m.MealTargetCalories);
        Assert.InRange(totalMealCalories, result.DailyPlanCalories * 0.3m, result.DailyPlanCalories * 1.2m);
    }

    private static Recipe Recipe(int id, decimal calories, decimal protein, decimal fat, decimal carbs) =>
        new()
        {
            RecipeId = id,
            RecipeCaloriesPerPortion = calories,
            RecipeProteinPerPortion = protein,
            RecipeFatPerPortion = fat,
            RecipeCarbsPerPortion = carbs,
            RecipeName = $"R{id}"
        };
}

