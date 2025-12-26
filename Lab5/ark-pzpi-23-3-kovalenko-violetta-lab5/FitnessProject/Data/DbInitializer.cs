using FitnessProject.Entities;
using FitnessProject.Enums;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace FitnessProject.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext context, bool forceRecreate = false)
    {
        if (forceRecreate)
        {
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
        }
        else if (await context.Users.AnyAsync())
        {
            return;
        }

        var users = CreateUsers();
        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();

        var userProfiles = CreateUserProfiles(users);
        await context.UserProfiles.AddRangeAsync(userProfiles);
        await context.SaveChangesAsync();

        var templateDietPlans = CreateTemplateDietPlans();
        await context.TemplateDietPlans.AddRangeAsync(templateDietPlans);
        await context.SaveChangesAsync();

        var products = CreateProducts();
        await context.Products.AddRangeAsync(products);
        await context.SaveChangesAsync();

        var recipes = CreateRecipes();
        await context.Recipes.AddRangeAsync(recipes);
        await context.SaveChangesAsync();

        var dailyDietPlans = CreateDailyDietPlans(users, templateDietPlans);
        await context.DailyDietPlans.AddRangeAsync(dailyDietPlans);
        await context.SaveChangesAsync();

        var meals = CreateMeals(dailyDietPlans);
        await context.Meals.AddRangeAsync(meals);
        await context.SaveChangesAsync();

        var recipeProducts = CreateRecipeProducts(recipes, products);
        await context.RecipeProducts.AddRangeAsync(recipeProducts);
        await context.SaveChangesAsync();

        var mealRecipes = CreateMealRecipes(meals, recipes);
        await context.MealRecipes.AddRangeAsync(mealRecipes);
        await context.SaveChangesAsync();

        var recommendations = CreateRecommendations(meals);
        await context.Recommendations.AddRangeAsync(recommendations);
        await context.SaveChangesAsync();

        var devices = CreateDevices(users);
        await context.Devices.AddRangeAsync(devices);
        await context.SaveChangesAsync();

        // Видаляємо всі старі телеметрії для користувача 1 (devices[0] та devices[1])
        // щоб залишити тільки дані, згенеровані IoT клієнтом
        if (devices.Count >= 2)
        {
            var user1DeviceIds = new[] { devices[0].DeviceId, devices[1].DeviceId };
            var oldTelemetry = await context.TelemetrySamples
                .Where(ts => user1DeviceIds.Contains(ts.DeviceId))
                .ToListAsync();
            
            if (oldTelemetry.Any())
            {
                context.TelemetrySamples.RemoveRange(oldTelemetry);
                await context.SaveChangesAsync();
            }
        }

        var telemetrySamples = CreateTelemetrySamples(devices);
        await context.TelemetrySamples.AddRangeAsync(telemetrySamples);
        await context.SaveChangesAsync();

        // Видаляємо всі старі записи сну для користувача 1
        if (devices.Count >= 2)
        {
            var user1DeviceIds = new[] { devices[0].DeviceId, devices[1].DeviceId };
            var oldSleepRecords = await context.SleepRecords
                .Where(sr => user1DeviceIds.Contains(sr.DeviceId))
                .ToListAsync();
            
            if (oldSleepRecords.Any())
            {
                context.SleepRecords.RemoveRange(oldSleepRecords);
                await context.SaveChangesAsync();
            }
        }

        var sleepRecords = CreateSleepRecords(devices);
        await context.SleepRecords.AddRangeAsync(sleepRecords);
        await context.SaveChangesAsync();

        // Видаляємо всі старі тренування для користувача 1
        if (devices.Count >= 2)
        {
            var user1DeviceIds = new[] { devices[0].DeviceId, devices[1].DeviceId };
            var oldTrainings = await context.TrainingSessions
                .Where(ts => user1DeviceIds.Contains(ts.DeviceId))
                .ToListAsync();
            
            if (oldTrainings.Any())
            {
                context.TrainingSessions.RemoveRange(oldTrainings);
                await context.SaveChangesAsync();
            }
        }

        var trainingSessions = CreateTrainingSessions(devices);
        await context.TrainingSessions.AddRangeAsync(trainingSessions);
        await context.SaveChangesAsync();
    }

    private static List<User> CreateUsers()
    {
        return new List<User>
        {
            new User
            {
                Email = "user1@example.com",
                PasswordHash = HashPassword("password123"),
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                Locale = "en-US",
                Role = UserRole.User
            },
            new User
            {
                Email = "admin@example.com",
                PasswordHash = HashPassword("admin123"),
                CreatedAt = DateTime.UtcNow.AddDays(-60),
                Locale = "en-US",
                Role = UserRole.Admin
            },
            new User
            {
                Email = "superadmin@example.com",
                PasswordHash = HashPassword("superadmin123"),
                CreatedAt = DateTime.UtcNow.AddDays(-90),
                Locale = "uk-UA",
                Role = UserRole.SuperAdmin
            },
            new User
            {
                Email = "admin2@example.com",
                PasswordHash = HashPassword("Admin2#Strong123"),
                CreatedAt = DateTime.UtcNow,
                Locale = "uk-UA",
                Role = UserRole.Admin
            },
            new User
            {
                Email = "diabetic.veg@example.com",
                PasswordHash = HashPassword("password123"),
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                Locale = "uk-UA",
                Role = UserRole.User
            },
            new User
            {
                Email = "celiac.gf@example.com",
                PasswordHash = HashPassword("password123"),
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                Locale = "en-US",
                Role = UserRole.User
            }
        };
    }

    private static List<UserProfile> CreateUserProfiles(List<User> users)
    {
        return new List<UserProfile>
        {
            new UserProfile
            {
                UserId = users[0].UserId,
                FirstName = "John",
                LastName = "Doe",
                Sex = Sex.Male,
                HeightCm = 180.5m,
                CurrentWeightKg = 75.0m,
                ActivityLevel = ActivityLevel.ModeratelyActive,
                GoalType = GoalType.WeightLoss,
                MedicalConditions = "{\"Allergens\":[\"Eggs\"],\"MedicalConditions\":[\"Diabetes\"],\"DietaryRestriction\":\"Vegetarian\"}",
                PreferredUnits = PreferredUnits.Metric,
                BirthDate = new DateTime(1990, 5, 15)
            },
            new UserProfile
            {
                UserId = users[1].UserId,
                FirstName = "Jane",
                LastName = "Smith",
                Sex = Sex.Female,
                HeightCm = 165.0m,
                CurrentWeightKg = 60.0m,
                ActivityLevel = ActivityLevel.VeryActive,
                GoalType = GoalType.WeightMaintenance,
                MedicalConditions = "{\"Allergens\":[\"Milk\"],\"MedicalConditions\":[\"Hypertension\"],\"DietaryRestriction\":\"Pescatarian\"}",
                PreferredUnits = PreferredUnits.Metric,
                BirthDate = new DateTime(1992, 8, 20)
            },
            new UserProfile
            {
                UserId = users[2].UserId,
                FirstName = "Admin",
                LastName = "User",
                Sex = Sex.Other,
                HeightCm = 175.0m,
                CurrentWeightKg = 70.0m,
                ActivityLevel = ActivityLevel.LightlyActive,
                GoalType = GoalType.HealthCorrection,
                MedicalConditions = "{\"Allergens\":[],\"MedicalConditions\":[\"KidneyDisease\"],\"DietaryRestriction\":\"None\"}",
                PreferredUnits = PreferredUnits.Imperial,
                BirthDate = new DateTime(1985, 3, 10)
            },
            new UserProfile
            {
                UserId = users[5].UserId,
                FirstName = "Admin2",
                LastName = "Panel",
                Sex = Sex.Female,
                HeightCm = 170.0m,
                CurrentWeightKg = 65.0m,
                ActivityLevel = ActivityLevel.Sedentary,
                GoalType = GoalType.WeightMaintenance,
                MedicalConditions = "{\"Allergens\":[],\"MedicalConditions\":[],\"DietaryRestriction\":\"None\"}",
                PreferredUnits = PreferredUnits.Metric,
                BirthDate = new DateTime(1991, 6, 1)
            },
            new UserProfile
            {
                UserId = users[3].UserId,
                FirstName = "Oleh",
                LastName = "Diabetic",
                Sex = Sex.Male,
                HeightCm = 178.0m,
                CurrentWeightKg = 82.0m,
                ActivityLevel = ActivityLevel.Sedentary,
                GoalType = GoalType.WeightLoss,
                MedicalConditions = "{\"Allergens\":[\"Sugar\"],\"MedicalConditions\":[\"Diabetes\",\"Hypertension\"],\"DietaryRestriction\":\"Vegan\"}",
                PreferredUnits = PreferredUnits.Metric,
                BirthDate = new DateTime(1988, 1, 5)
            },
            new UserProfile
            {
                UserId = users[4].UserId,
                FirstName = "Anna",
                LastName = "GlutenFree",
                Sex = Sex.Female,
                HeightCm = 170.0m,
                CurrentWeightKg = 65.0m,
                ActivityLevel = ActivityLevel.LightlyActive,
                GoalType = GoalType.WeightMaintenance,
                MedicalConditions = "{\"Allergens\":[\"Gluten\"],\"MedicalConditions\":[\"CeliacDisease\"],\"DietaryRestriction\":\"GlutenFree\"}",
                PreferredUnits = PreferredUnits.Metric,
                BirthDate = new DateTime(1995, 4, 12)
            }
        };
    }

    private static List<TemplateDietPlan> CreateTemplateDietPlans()
    {
        return new List<TemplateDietPlan>
        {
            new TemplateDietPlan
            {
                TemplateName = "Weight Loss Plan",
                TemplateDescription = "A balanced diet plan for weight loss",
                TemplateCaloriesMin = 1500m,
                TemplateCaloriesMax = 1800m,
                TemplateProteinMin = 100m,
                TemplateProteinMax = 120m,
                TemplateFatMin = 40m,
                TemplateFatMax = 60m,
                TemplateCarbsMin = 150m,
                TemplateCarbsMax = 200m,
                TemplateNumberOfMeals = 5,
                TemplateStatus = TemplateStatus.Active,
                TemplateCreatedAt = DateTime.UtcNow.AddDays(-20)
            },
            new TemplateDietPlan
            {
                TemplateName = "Muscle Gain Plan",
                TemplateDescription = "High protein plan for muscle building",
                TemplateCaloriesMin = 2500m,
                TemplateCaloriesMax = 3000m,
                TemplateProteinMin = 150m,
                TemplateProteinMax = 200m,
                TemplateFatMin = 60m,
                TemplateFatMax = 80m,
                TemplateCarbsMin = 250m,
                TemplateCarbsMax = 350m,
                TemplateNumberOfMeals = 6,
                TemplateStatus = TemplateStatus.Active,
                TemplateCreatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new TemplateDietPlan
            {
                TemplateName = "Maintenance Plan",
                TemplateDescription = "Balanced maintenance diet",
                TemplateCaloriesMin = 2000m,
                TemplateCaloriesMax = 2200m,
                TemplateProteinMin = 120m,
                TemplateProteinMax = 140m,
                TemplateFatMin = 50m,
                TemplateFatMax = 70m,
                TemplateCarbsMin = 200m,
                TemplateCarbsMax = 250m,
                TemplateNumberOfMeals = 4,
                TemplateStatus = TemplateStatus.Archived,
                TemplateCreatedAt = DateTime.UtcNow.AddDays(-10)
            }
        };
    }

    private static List<Product> CreateProducts()
    {
        return new List<Product>
        {
            new Product
            {
                ProductName = "Chicken Breast",
                CaloriesPer100g = 165m,
                ProteinPer100g = 31m,
                FatPer100g = 3.6m,
                CarbsPer100g = 0m,
                Restriction = "high_protein",
                Allergens = null,
                Unit = "g",
                Tags = ProductTags.Meat | ProductTags.HighProtein
            },
            new Product
            {
                ProductName = "Brown Rice",
                CaloriesPer100g = 111m,
                ProteinPer100g = 2.6m,
                FatPer100g = 0.9m,
                CarbsPer100g = 23m,
                Restriction = "high_gi",
                Allergens = null,
                Unit = "g",
                Tags = ProductTags.HighGI | ProductTags.Gluten
            },
            new Product
            {
                ProductName = "Broccoli",
                CaloriesPer100g = 34m,
                ProteinPer100g = 2.8m,
                FatPer100g = 0.4m,
                CarbsPer100g = 7m,
                Restriction = null,
                Allergens = null,
                Unit = "g",
                Tags = ProductTags.None
            },
            new Product
            {
                ProductName = "Salmon",
                CaloriesPer100g = 208m,
                ProteinPer100g = 20m,
                FatPer100g = 12m,
                CarbsPer100g = 0m,
                Restriction = "omega3,high_protein",
                Allergens = "Fish",
                Unit = "g",
                Tags = ProductTags.Fish | ProductTags.HighProtein
            },
            new Product
            {
                ProductName = "Eggs",
                CaloriesPer100g = 155m,
                ProteinPer100g = 13m,
                FatPer100g = 11m,
                CarbsPer100g = 1.1m,
                Restriction = "high_cholesterol",
                Allergens = "Eggs",
                Unit = "piece",
                Tags = ProductTags.Egg | ProductTags.HighProtein
            },
            new Product
            {
                ProductName = "Oatmeal",
                CaloriesPer100g = 389m,
                ProteinPer100g = 17m,
                FatPer100g = 7m,
                CarbsPer100g = 66m,
                Restriction = "whole_grain,gluten",
                Allergens = "Gluten",
                Unit = "g",
                Tags = ProductTags.Gluten | ProductTags.HighGI
            },
            new Product
            {
                ProductName = "Skim Milk",
                CaloriesPer100g = 35m,
                ProteinPer100g = 3.4m,
                FatPer100g = 0.2m,
                CarbsPer100g = 5m,
                Restriction = "dairy",
                Allergens = "Milk",
                Unit = "ml",
                Tags = ProductTags.Dairy
            },
            new Product
            {
                ProductName = "Greek Yogurt",
                CaloriesPer100g = 59m,
                ProteinPer100g = 10m,
                FatPer100g = 0.4m,
                CarbsPer100g = 3.6m,
                Restriction = "dairy,high_protein",
                Allergens = "Milk",
                Unit = "g",
                Tags = ProductTags.Dairy | ProductTags.HighProtein
            },
            new Product
            {
                ProductName = "Almonds",
                CaloriesPer100g = 579m,
                ProteinPer100g = 21m,
                FatPer100g = 50m,
                CarbsPer100g = 22m,
                Restriction = "high_fat",
                Allergens = "Nuts",
                Unit = "g",
                Tags = ProductTags.TreeNut | ProductTags.HighProtein
            },
            new Product
            {
                ProductName = "Peanut Butter",
                CaloriesPer100g = 588m,
                ProteinPer100g = 25m,
                FatPer100g = 50m,
                CarbsPer100g = 20m,
                Restriction = "high_fat",
                Allergens = "Peanuts",
                Unit = "g",
                Tags = ProductTags.Peanut | ProductTags.HighProtein
            },
            new Product
            {
                ProductName = "Whole Wheat Bread",
                CaloriesPer100g = 247m,
                ProteinPer100g = 13m,
                FatPer100g = 4.2m,
                CarbsPer100g = 41m,
                Restriction = "wheat,gluten",
                Allergens = "Gluten",
                Unit = "g",
                Tags = ProductTags.Gluten | ProductTags.HighGI
            },
            new Product
            {
                ProductName = "Gluten-Free Bread",
                CaloriesPer100g = 240m,
                ProteinPer100g = 6m,
                FatPer100g = 4m,
                CarbsPer100g = 46m,
                Restriction = "gluten_free",
                Allergens = null,
                Unit = "g",
                Tags = ProductTags.None
            },
            new Product
            {
                ProductName = "Tofu",
                CaloriesPer100g = 76m,
                ProteinPer100g = 8m,
                FatPer100g = 4.8m,
                CarbsPer100g = 1.9m,
                Restriction = "soy,plant_protein",
                Allergens = null,
                Unit = "g",
                Tags = ProductTags.Soy | ProductTags.HighProtein
            },
            new Product
            {
                ProductName = "Apple",
                CaloriesPer100g = 52m,
                ProteinPer100g = 0.3m,
                FatPer100g = 0.2m,
                CarbsPer100g = 14m,
                Restriction = "fruit",
                Allergens = null,
                Unit = "g",
                Tags = ProductTags.None
            },
            new Product
            {
                ProductName = "Banana",
                CaloriesPer100g = 89m,
                ProteinPer100g = 1.1m,
                FatPer100g = 0.3m,
                CarbsPer100g = 23m,
                Restriction = "fruit,high_gi",
                Allergens = null,
                Unit = "g",
                Tags = ProductTags.HighGI | ProductTags.Sugar
            },
            new Product
            {
                ProductName = "Blueberries",
                CaloriesPer100g = 57m,
                ProteinPer100g = 0.7m,
                FatPer100g = 0.3m,
                CarbsPer100g = 14m,
                Restriction = "fruit,low_gi",
                Allergens = null,
                Unit = "g",
                Tags = ProductTags.None
            },
            new Product
            {
                ProductName = "Lentils",
                CaloriesPer100g = 116m,
                ProteinPer100g = 9m,
                FatPer100g = 0.4m,
                CarbsPer100g = 20m,
                Restriction = "legume,high_protein",
                Allergens = null,
                Unit = "g",
                Tags = ProductTags.Legume | ProductTags.HighProtein
            },
            new Product
            {
                ProductName = "Chickpeas",
                CaloriesPer100g = 164m,
                ProteinPer100g = 9m,
                FatPer100g = 2.6m,
                CarbsPer100g = 27m,
                Restriction = "legume,high_protein",
                Allergens = null,
                Unit = "g",
                Tags = ProductTags.Legume | ProductTags.HighProtein
            },
            new Product
            {
                ProductName = "Spinach",
                CaloriesPer100g = 23m,
                ProteinPer100g = 2.9m,
                FatPer100g = 0.4m,
                CarbsPer100g = 3.6m,
                Restriction = "leafy_green",
                Allergens = null,
                Unit = "g",
                Tags = ProductTags.None
            },
            new Product
            {
                ProductName = "Sweet Potato",
                CaloriesPer100g = 86m,
                ProteinPer100g = 1.6m,
                FatPer100g = 0.1m,
                CarbsPer100g = 20m,
                Restriction = "root,medium_gi",
                Allergens = null,
                Unit = "g",
                Tags = ProductTags.HighGI
            },
            new Product
            {
                ProductName = "Quinoa",
                CaloriesPer100g = 120m,
                ProteinPer100g = 4.4m,
                FatPer100g = 1.9m,
                CarbsPer100g = 21.0m,
                Restriction = "grain_gf,low_gi",
                Allergens = null,
                Unit = "g",
                Tags = ProductTags.HighProtein
            },
            new Product
            {
                ProductName = "Buckwheat Groats",
                CaloriesPer100g = 110m,
                ProteinPer100g = 4m,
                FatPer100g = 1m,
                CarbsPer100g = 23m,
                Restriction = "grain_gf,low_gi",
                Allergens = null,
                Unit = "g",
                Tags = ProductTags.HighProtein
            },
            new Product
            {
                ProductName = "Coconut Milk",
                CaloriesPer100g = 230m,
                ProteinPer100g = 2m,
                FatPer100g = 24m,
                CarbsPer100g = 6m,
                Restriction = "plant_milk",
                Allergens = null,
                Unit = "ml",
                Tags = ProductTags.PlantMilk
            },
            new Product
            {
                ProductName = "Chia Seeds",
                CaloriesPer100g = 486m,
                ProteinPer100g = 17m,
                FatPer100g = 31m,
                CarbsPer100g = 42m,
                Restriction = "plant_fat_fiber",
                Allergens = null,
                Unit = "g",
                Tags = ProductTags.HighProtein
            },
            new Product
            {
                ProductName = "Bell Pepper",
                CaloriesPer100g = 31m,
                ProteinPer100g = 1m,
                FatPer100g = 0.3m,
                CarbsPer100g = 6m,
                Restriction = "vegetable_low_gi",
                Allergens = null,
                Unit = "g",
                Tags = ProductTags.None
            },
            new Product
            {
                ProductName = "Tomato",
                CaloriesPer100g = 18m,
                ProteinPer100g = 0.9m,
                FatPer100g = 0.2m,
                CarbsPer100g = 3.9m,
                Restriction = "vegetable_low_gi",
                Allergens = null,
                Unit = "g",
                Tags = ProductTags.None
            },
            new Product
            {
                ProductName = "Zucchini",
                CaloriesPer100g = 17m,
                ProteinPer100g = 1.2m,
                FatPer100g = 0.3m,
                CarbsPer100g = 3.1m,
                Restriction = "vegetable_low_gi",
                Allergens = null,
                Unit = "g",
                Tags = ProductTags.None
            },
            new Product
            {
                ProductName = "Cauliflower",
                CaloriesPer100g = 25m,
                ProteinPer100g = 1.9m,
                FatPer100g = 0.3m,
                CarbsPer100g = 5m,
                Restriction = "vegetable_low_gi",
                Allergens = null,
                Unit = "g",
                Tags = ProductTags.None
            }
        };
    }

    private static List<Recipe> CreateRecipes()
    {
        return new List<Recipe>
        {
            new Recipe
            {
                RecipeName = "Grilled Chicken with Rice",
                RecipeInstructions = "Season chicken, grill, cook brown rice, serve together.",
                RecipeCaloriesPerPortion = 450m,
                RecipeFatPerPortion = 8m,
                RecipeCarbsPerPortion = 45m,
                RecipeProteinPerPortion = 45m,
                RecipeProductsGrams = 300m
            },
            new Recipe
            {
                RecipeName = "Salmon with Broccoli",
                RecipeInstructions = "Bake salmon, steam broccoli, serve together.",
                RecipeCaloriesPerPortion = 380m,
                RecipeFatPerPortion = 18m,
                RecipeCarbsPerPortion = 12m,
                RecipeProteinPerPortion = 35m,
                RecipeProductsGrams = 250m
            },
            new Recipe
            {
                RecipeName = "Scrambled Eggs",
                RecipeInstructions = "Beat eggs, cook on pan, season.",
                RecipeCaloriesPerPortion = 280m,
                RecipeFatPerPortion = 20m,
                RecipeCarbsPerPortion = 2m,
                RecipeProteinPerPortion = 20m,
                RecipeProductsGrams = 150m
            },
            new Recipe
            {
                RecipeName = "Oatmeal Bowl",
                RecipeInstructions = "Cook oatmeal, add fruits and nuts.",
                RecipeCaloriesPerPortion = 250m,
                RecipeFatPerPortion = 5m,
                RecipeCarbsPerPortion = 45m,
                RecipeProteinPerPortion = 8m,
                RecipeProductsGrams = 100m
            },
            new Recipe
            {
                RecipeName = "Vegan Lentil Stew",
                RecipeInstructions = "Simmer lentils with vegetables and spices.",
                RecipeCaloriesPerPortion = 320m,
                RecipeFatPerPortion = 5m,
                RecipeCarbsPerPortion = 45m,
                RecipeProteinPerPortion = 18m,
                RecipeProductsGrams = 300m
            },
            new Recipe
            {
                RecipeName = "Gluten-Free Chickpea Salad",
                RecipeInstructions = "Mix chickpeas with vegetables and olive oil.",
                RecipeCaloriesPerPortion = 280m,
                RecipeFatPerPortion = 12m,
                RecipeCarbsPerPortion = 30m,
                RecipeProteinPerPortion = 12m,
                RecipeProductsGrams = 200m
            },
            new Recipe
            {
                RecipeName = "Tofu and Veggie Stir-Fry",
                RecipeInstructions = "Stir-fry tofu with broccoli and spinach.",
                RecipeCaloriesPerPortion = 300m,
                RecipeFatPerPortion = 12m,
                RecipeCarbsPerPortion = 20m,
                RecipeProteinPerPortion = 20m,
                RecipeProductsGrams = 250m
            },
            new Recipe
            {
                RecipeName = "Fruit Yogurt Bowl",
                RecipeInstructions = "Combine Greek yogurt with banana and berries.",
                RecipeCaloriesPerPortion = 220m,
                RecipeFatPerPortion = 4m,
                RecipeCarbsPerPortion = 35m,
                RecipeProteinPerPortion = 12m,
                RecipeProductsGrams = 200m
            },
            new Recipe
            {
                RecipeName = "Quinoa Veggie Bowl",
                RecipeInstructions = "Cook quinoa, mix with chickpeas, spinach, and bell pepper.",
                RecipeCaloriesPerPortion = 420m,
                RecipeFatPerPortion = 10m,
                RecipeCarbsPerPortion = 60m,
                RecipeProteinPerPortion = 18m,
                RecipeProductsGrams = 350m
            },
            new Recipe
            {
                RecipeName = "Tofu Buckwheat Stir-Fry",
                RecipeInstructions = "Stir-fry tofu with buckwheat, broccoli, and bell pepper.",
                RecipeCaloriesPerPortion = 380m,
                RecipeFatPerPortion = 12m,
                RecipeCarbsPerPortion = 40m,
                RecipeProteinPerPortion = 22m,
                RecipeProductsGrams = 350m
            },
            new Recipe
            {
                RecipeName = "Chia Coconut Pudding",
                RecipeInstructions = "Soak chia seeds in coconut milk, top with berries.",
                RecipeCaloriesPerPortion = 280m,
                RecipeFatPerPortion = 18m,
                RecipeCarbsPerPortion = 25m,
                RecipeProteinPerPortion = 6m,
                RecipeProductsGrams = 230m
            },
            new Recipe
            {
                RecipeName = "Zucchini Lentil Ragu",
                RecipeInstructions = "Simmer lentils with tomatoes and zucchini.",
                RecipeCaloriesPerPortion = 350m,
                RecipeFatPerPortion = 6m,
                RecipeCarbsPerPortion = 50m,
                RecipeProteinPerPortion = 18m,
                RecipeProductsGrams = 350m
            },
            new Recipe
            {
                RecipeName = "Cauliflower Rice with Tofu",
                RecipeInstructions = "Stir-fry cauliflower rice with tofu and spinach.",
                RecipeCaloriesPerPortion = 300m,
                RecipeFatPerPortion = 10m,
                RecipeCarbsPerPortion = 25m,
                RecipeProteinPerPortion = 20m,
                RecipeProductsGrams = 320m
            },
            new Recipe
            {
                RecipeName = "Salmon Quinoa Bowl",
                RecipeInstructions = "Bake salmon, serve with quinoa and broccoli.",
                RecipeCaloriesPerPortion = 420m,
                RecipeFatPerPortion = 18m,
                RecipeCarbsPerPortion = 35m,
                RecipeProteinPerPortion = 32m,
                RecipeProductsGrams = 320m
            },
            new Recipe
            {
                RecipeName = "Quinoa Breakfast Bowl",
                RecipeInstructions = "Cook quinoa, add blueberries and chia seeds.",
                RecipeCaloriesPerPortion = 320m,
                RecipeFatPerPortion = 8m,
                RecipeCarbsPerPortion = 45m,
                RecipeProteinPerPortion = 12m,
                RecipeProductsGrams = 200m
            },
            new Recipe
            {
                RecipeName = "Lentil and Vegetable Soup",
                RecipeInstructions = "Simmer lentils with tomatoes, zucchini, and bell pepper.",
                RecipeCaloriesPerPortion = 280m,
                RecipeFatPerPortion = 4m,
                RecipeCarbsPerPortion = 40m,
                RecipeProteinPerPortion = 16m,
                RecipeProductsGrams = 350m
            },
            new Recipe
            {
                RecipeName = "Tofu Scramble with Vegetables",
                RecipeInstructions = "Stir-fry tofu with bell pepper, tomato, and spinach.",
                RecipeCaloriesPerPortion = 260m,
                RecipeFatPerPortion = 10m,
                RecipeCarbsPerPortion = 18m,
                RecipeProteinPerPortion = 22m,
                RecipeProductsGrams = 280m
            },
            new Recipe
            {
                RecipeName = "Chickpea and Spinach Curry",
                RecipeInstructions = "Cook chickpeas with spinach, tomatoes, and spices.",
                RecipeCaloriesPerPortion = 340m,
                RecipeFatPerPortion = 8m,
                RecipeCarbsPerPortion = 45m,
                RecipeProteinPerPortion = 16m,
                RecipeProductsGrams = 300m
            },
            new Recipe
            {
                RecipeName = "Buckwheat with Tofu and Vegetables",
                RecipeInstructions = "Cook buckwheat, serve with stir-fried tofu, broccoli, and bell pepper.",
                RecipeCaloriesPerPortion = 400m,
                RecipeFatPerPortion = 12m,
                RecipeCarbsPerPortion = 50m,
                RecipeProteinPerPortion = 24m,
                RecipeProductsGrams = 380m
            },
            new Recipe
            {
                RecipeName = "Lentil Salad with Vegetables",
                RecipeInstructions = "Mix cooked lentils with bell pepper, tomato, and zucchini.",
                RecipeCaloriesPerPortion = 240m,
                RecipeFatPerPortion = 6m,
                RecipeCarbsPerPortion = 35m,
                RecipeProteinPerPortion = 14m,
                RecipeProductsGrams = 250m
            },
            new Recipe
            {
                RecipeName = "Tofu and Quinoa Power Bowl",
                RecipeInstructions = "Combine quinoa with tofu, spinach, and bell pepper.",
                RecipeCaloriesPerPortion = 380m,
                RecipeFatPerPortion = 10m,
                RecipeCarbsPerPortion = 42m,
                RecipeProteinPerPortion = 26m,
                RecipeProductsGrams = 320m
            },
            new Recipe
            {
                RecipeName = "Chickpea and Cauliflower Stew",
                RecipeInstructions = "Simmer chickpeas with cauliflower, tomatoes, and zucchini.",
                RecipeCaloriesPerPortion = 300m,
                RecipeFatPerPortion = 7m,
                RecipeCarbsPerPortion = 42m,
                RecipeProteinPerPortion = 15m,
                RecipeProductsGrams = 320m
            },
            new Recipe
            {
                RecipeName = "Apple and Blueberry Snack",
                RecipeInstructions = "Fresh apple slices with blueberries.",
                RecipeCaloriesPerPortion = 120m,
                RecipeFatPerPortion = 0.5m,
                RecipeCarbsPerPortion = 28m,
                RecipeProteinPerPortion = 1m,
                RecipeProductsGrams = 200m
            }
        };
    }

    private static List<DailyDietPlan> CreateDailyDietPlans(List<User> users, List<TemplateDietPlan> templates)
    {
        return new List<DailyDietPlan>
        {
            new DailyDietPlan
            {
                UserId = users[0].UserId,
                TemplateDietPlanId = templates[0].TemplateDietPlanId,
                DailyDietPlanName = "Monday Weight Loss Plan",
                DailyPlanDescription = "Day 1 of weight loss program",
                DailyPlanCalories = 1650m,
                DailyPlanFat = 50m,
                DailyPlanCarbs = 175m,
                DailyPlanProtein = 110m,
                DailyPlanNumberOfMeals = 5,
                DailyPlanStatus = DailyPlanStatus.InProgress,
                DailyPlanCreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new DailyDietPlan
            {
                UserId = users[0].UserId,
                TemplateDietPlanId = templates[0].TemplateDietPlanId,
                DailyDietPlanName = "Tuesday Weight Loss Plan",
                DailyPlanDescription = "Day 2 of weight loss program",
                DailyPlanCalories = 1700m,
                DailyPlanFat = 55m,
                DailyPlanCarbs = 180m,
                DailyPlanProtein = 115m,
                DailyPlanNumberOfMeals = 5,
                DailyPlanStatus = DailyPlanStatus.Planned,
                DailyPlanCreatedAt = DateTime.UtcNow
            },
            new DailyDietPlan
            {
                UserId = users[1].UserId,
                TemplateDietPlanId = templates[1].TemplateDietPlanId,
                DailyDietPlanName = "Monday Muscle Gain Plan",
                DailyPlanDescription = "Day 1 of muscle building program",
                DailyPlanCalories = 2750m,
                DailyPlanFat = 70m,
                DailyPlanCarbs = 300m,
                DailyPlanProtein = 175m,
                DailyPlanNumberOfMeals = 6,
                DailyPlanStatus = DailyPlanStatus.Completed,
                DailyPlanCreatedAt = DateTime.UtcNow.AddDays(-2)
            }
        };
    }

    private static List<Meal> CreateMeals(List<DailyDietPlan> dailyDietPlans)
    {
        var meals = new List<Meal>();

        meals.Add(new Meal
        {
            DailyDietPlanId = dailyDietPlans[0].DailyDietPlanId,
            MealTime = MealTime.Breakfast,
            MealOrder = 1,
            MealTargetCalories = 350m,
            MealTargetFat = 10m,
            MealTargetCarbs = 45m,
            MealTargetProtein = 25m
        });

        meals.Add(new Meal
        {
            DailyDietPlanId = dailyDietPlans[0].DailyDietPlanId,
            MealTime = MealTime.Snack,
            MealOrder = 2,
            MealTargetCalories = 150m,
            MealTargetFat = 5m,
            MealTargetCarbs = 20m,
            MealTargetProtein = 10m
        });

        meals.Add(new Meal
        {
            DailyDietPlanId = dailyDietPlans[0].DailyDietPlanId,
            MealTime = MealTime.Lunch,
            MealOrder = 3,
            MealTargetCalories = 500m,
            MealTargetFat = 15m,
            MealTargetCarbs = 50m,
            MealTargetProtein = 35m
        });

        meals.Add(new Meal
        {
            DailyDietPlanId = dailyDietPlans[0].DailyDietPlanId,
            MealTime = MealTime.Snack,
            MealOrder = 4,
            MealTargetCalories = 200m,
            MealTargetFat = 8m,
            MealTargetCarbs = 25m,
            MealTargetProtein = 15m
        });

        meals.Add(new Meal
        {
            DailyDietPlanId = dailyDietPlans[0].DailyDietPlanId,
            MealTime = MealTime.Dinner,
            MealOrder = 5,
            MealTargetCalories = 450m,
            MealTargetFat = 12m,
            MealTargetCarbs = 35m,
            MealTargetProtein = 25m
        });

        meals.Add(new Meal
        {
            DailyDietPlanId = dailyDietPlans[1].DailyDietPlanId,
            MealTime = MealTime.Breakfast,
            MealOrder = 1,
            MealTargetCalories = 400m,
            MealTargetFat = 12m,
            MealTargetCarbs = 50m,
            MealTargetProtein = 30m
        });

        meals.Add(new Meal
        {
            DailyDietPlanId = dailyDietPlans[1].DailyDietPlanId,
            MealTime = MealTime.Lunch,
            MealOrder = 2,
            MealTargetCalories = 550m,
            MealTargetFat = 18m,
            MealTargetCarbs = 60m,
            MealTargetProtein = 40m
        });

        meals.Add(new Meal
        {
            DailyDietPlanId = dailyDietPlans[2].DailyDietPlanId,
            MealTime = MealTime.Breakfast,
            MealOrder = 1,
            MealTargetCalories = 500m,
            MealTargetFat = 15m,
            MealTargetCarbs = 60m,
            MealTargetProtein = 35m
        });

        meals.Add(new Meal
        {
            DailyDietPlanId = dailyDietPlans[2].DailyDietPlanId,
            MealTime = MealTime.PreWorkout,
            MealOrder = 2,
            MealTargetCalories = 300m,
            MealTargetFat = 5m,
            MealTargetCarbs = 50m,
            MealTargetProtein = 15m
        });

        meals.Add(new Meal
        {
            DailyDietPlanId = dailyDietPlans[2].DailyDietPlanId,
            MealTime = MealTime.PostWorkout,
            MealOrder = 3,
            MealTargetCalories = 400m,
            MealTargetFat = 10m,
            MealTargetCarbs = 45m,
            MealTargetProtein = 40m
        });

        meals.Add(new Meal
        {
            DailyDietPlanId = dailyDietPlans[2].DailyDietPlanId,
            MealTime = MealTime.Lunch,
            MealOrder = 4,
            MealTargetCalories = 600m,
            MealTargetFat = 20m,
            MealTargetCarbs = 70m,
            MealTargetProtein = 45m
        });

        meals.Add(new Meal
        {
            DailyDietPlanId = dailyDietPlans[2].DailyDietPlanId,
            MealTime = MealTime.Snack,
            MealOrder = 5,
            MealTargetCalories = 350m,
            MealTargetFat = 12m,
            MealTargetCarbs = 40m,
            MealTargetProtein = 25m
        });

        meals.Add(new Meal
        {
            DailyDietPlanId = dailyDietPlans[2].DailyDietPlanId,
            MealTime = MealTime.Dinner,
            MealOrder = 6,
            MealTargetCalories = 600m,
            MealTargetFat = 18m,
            MealTargetCarbs = 35m,
            MealTargetProtein = 40m
        });

        return meals;
    }

    private static List<RecipeProduct> CreateRecipeProducts(List<Recipe> recipes, List<Product> products)
    {
        return new List<RecipeProduct>
        {
            new RecipeProduct
            {
                RecipeId = recipes[0].RecipeId,
                ProductId = products[0].ProductId,
                QuantityGrams = 150m
            },
            new RecipeProduct
            {
                RecipeId = recipes[0].RecipeId,
                ProductId = products[1].ProductId,
                QuantityGrams = 150m
            },
            new RecipeProduct
            {
                RecipeId = recipes[1].RecipeId,
                ProductId = products[3].ProductId,
                QuantityGrams = 150m
            },
            new RecipeProduct
            {
                RecipeId = recipes[1].RecipeId,
                ProductId = products[2].ProductId,
                QuantityGrams = 100m
            },
            new RecipeProduct
            {
                RecipeId = recipes[2].RecipeId,
                ProductId = products[4].ProductId,
                QuantityGrams = 150m
            },
            new RecipeProduct
            {
                RecipeId = recipes[3].RecipeId,
                ProductId = products[5].ProductId,
                QuantityGrams = 50m
            },
            new RecipeProduct
            {
                RecipeId = recipes[4].RecipeId,
                ProductId = products[15].ProductId, // Lentils
                QuantityGrams = 150m
            },
            new RecipeProduct
            {
                RecipeId = recipes[4].RecipeId,
                ProductId = products[18].ProductId, // Spinach
                QuantityGrams = 100m
            },
            new RecipeProduct
            {
                RecipeId = recipes[5].RecipeId,
                ProductId = products[16].ProductId, // Chickpeas
                QuantityGrams = 150m
            },
            new RecipeProduct
            {
                RecipeId = recipes[5].RecipeId,
                ProductId = products[18].ProductId, // Spinach
                QuantityGrams = 50m
            },
            new RecipeProduct
            {
                RecipeId = recipes[6].RecipeId,
                ProductId = products[12].ProductId, // Tofu
                QuantityGrams = 150m
            },
            new RecipeProduct
            {
                RecipeId = recipes[6].RecipeId,
                ProductId = products[2].ProductId, // Broccoli
                QuantityGrams = 80m
            },
            new RecipeProduct
            {
                RecipeId = recipes[6].RecipeId,
                ProductId = products[18].ProductId, // Spinach
                QuantityGrams = 70m
            },
            new RecipeProduct
            {
                RecipeId = recipes[7].RecipeId,
                ProductId = products[7].ProductId, // Greek Yogurt
                QuantityGrams = 150m
            },
            new RecipeProduct
            {
                RecipeId = recipes[7].RecipeId,
                ProductId = products[14].ProductId, // Banana
                QuantityGrams = 50m
            },
            new RecipeProduct
            {
                RecipeId = recipes[7].RecipeId,
                ProductId = products[15].ProductId, // Blueberries
                QuantityGrams = 50m
            },
            new RecipeProduct
            {
                RecipeId = recipes[8].RecipeId, // Quinoa Veggie Bowl
                ProductId = products[20].ProductId, // Quinoa
                QuantityGrams = 150m
            },
            new RecipeProduct
            {
                RecipeId = recipes[8].RecipeId,
                ProductId = products[17].ProductId, // Chickpeas
                QuantityGrams = 100m
            },
            new RecipeProduct
            {
                RecipeId = recipes[8].RecipeId,
                ProductId = products[18].ProductId, // Spinach
                QuantityGrams = 50m
            },
            new RecipeProduct
            {
                RecipeId = recipes[8].RecipeId,
                ProductId = products[24].ProductId, // Bell Pepper
                QuantityGrams = 50m
            },
            new RecipeProduct
            {
                RecipeId = recipes[9].RecipeId, // Tofu Buckwheat Stir-Fry
                ProductId = products[12].ProductId, // Tofu
                QuantityGrams = 150m
            },
            new RecipeProduct
            {
                RecipeId = recipes[9].RecipeId,
                ProductId = products[21].ProductId, // Buckwheat Groats
                QuantityGrams = 120m
            },
            new RecipeProduct
            {
                RecipeId = recipes[9].RecipeId,
                ProductId = products[2].ProductId, // Broccoli
                QuantityGrams = 80m
            },
            new RecipeProduct
            {
                RecipeId = recipes[9].RecipeId,
                ProductId = products[24].ProductId, // Bell Pepper
                QuantityGrams = 50m
            },
            new RecipeProduct
            {
                RecipeId = recipes[10].RecipeId, // Chia Coconut Pudding
                ProductId = products[22].ProductId, // Coconut Milk
                QuantityGrams = 150m
            },
            new RecipeProduct
            {
                RecipeId = recipes[10].RecipeId,
                ProductId = products[23].ProductId, // Chia Seeds
                QuantityGrams = 30m
            },
            new RecipeProduct
            {
                RecipeId = recipes[10].RecipeId,
                ProductId = products[15].ProductId, // Blueberries
                QuantityGrams = 50m
            },
            new RecipeProduct
            {
                RecipeId = recipes[11].RecipeId, // Zucchini Lentil Ragu
                ProductId = products[16].ProductId, // Lentils
                QuantityGrams = 150m
            },
            new RecipeProduct
            {
                RecipeId = recipes[11].RecipeId,
                ProductId = products[26].ProductId, // Zucchini
                QuantityGrams = 150m
            },
            new RecipeProduct
            {
                RecipeId = recipes[11].RecipeId,
                ProductId = products[25].ProductId, // Tomato
                QuantityGrams = 100m
            },
            new RecipeProduct
            {
                RecipeId = recipes[12].RecipeId, // Cauliflower Rice with Tofu
                ProductId = products[27].ProductId, // Cauliflower
                QuantityGrams = 200m
            },
            new RecipeProduct
            {
                RecipeId = recipes[12].RecipeId,
                ProductId = products[12].ProductId, // Tofu
                QuantityGrams = 150m
            },
            new RecipeProduct
            {
                RecipeId = recipes[12].RecipeId,
                ProductId = products[18].ProductId, // Spinach
                QuantityGrams = 50m
            },
            new RecipeProduct
            {
                RecipeId = recipes[13].RecipeId, // Salmon Quinoa Bowl
                ProductId = products[3].ProductId, // Salmon
                QuantityGrams = 150m
            },
            new RecipeProduct
            {
                RecipeId = recipes[13].RecipeId,
                ProductId = products[20].ProductId, // Quinoa
                QuantityGrams = 120m
            },
            new RecipeProduct
            {
                RecipeId = recipes[13].RecipeId,
                ProductId = products[2].ProductId, // Broccoli
                QuantityGrams = 80m
            },
            new RecipeProduct
            {
                RecipeId = recipes[14].RecipeId, // Quinoa Breakfast Bowl
                ProductId = products[20].ProductId, // Quinoa
                QuantityGrams = 100m
            },
            new RecipeProduct
            {
                RecipeId = recipes[14].RecipeId,
                ProductId = products[15].ProductId, // Blueberries
                QuantityGrams = 50m
            },
            new RecipeProduct
            {
                RecipeId = recipes[14].RecipeId,
                ProductId = products[23].ProductId, // Chia Seeds
                QuantityGrams = 20m
            },
            new RecipeProduct
            {
                RecipeId = recipes[15].RecipeId, // Lentil and Vegetable Soup
                ProductId = products[16].ProductId, // Lentils
                QuantityGrams = 120m
            },
            new RecipeProduct
            {
                RecipeId = recipes[15].RecipeId,
                ProductId = products[25].ProductId, // Tomato
                QuantityGrams = 100m
            },
            new RecipeProduct
            {
                RecipeId = recipes[15].RecipeId,
                ProductId = products[26].ProductId, // Zucchini
                QuantityGrams = 80m
            },
            new RecipeProduct
            {
                RecipeId = recipes[15].RecipeId,
                ProductId = products[24].ProductId, // Bell Pepper
                QuantityGrams = 50m
            },
            new RecipeProduct
            {
                RecipeId = recipes[16].RecipeId, // Tofu Scramble with Vegetables
                ProductId = products[12].ProductId, // Tofu
                QuantityGrams = 150m
            },
            new RecipeProduct
            {
                RecipeId = recipes[16].RecipeId,
                ProductId = products[24].ProductId, // Bell Pepper
                QuantityGrams = 60m
            },
            new RecipeProduct
            {
                RecipeId = recipes[16].RecipeId,
                ProductId = products[25].ProductId, // Tomato
                QuantityGrams = 50m
            },
            new RecipeProduct
            {
                RecipeId = recipes[16].RecipeId,
                ProductId = products[18].ProductId, // Spinach
                QuantityGrams = 50m
            },
            new RecipeProduct
            {
                RecipeId = recipes[17].RecipeId, // Chickpea and Spinach Curry
                ProductId = products[17].ProductId, // Chickpeas
                QuantityGrams = 150m
            },
            new RecipeProduct
            {
                RecipeId = recipes[17].RecipeId,
                ProductId = products[18].ProductId, // Spinach
                QuantityGrams = 100m
            },
            new RecipeProduct
            {
                RecipeId = recipes[17].RecipeId,
                ProductId = products[25].ProductId, // Tomato
                QuantityGrams = 50m
            },
            new RecipeProduct
            {
                RecipeId = recipes[18].RecipeId, // Buckwheat with Tofu and Vegetables
                ProductId = products[21].ProductId, // Buckwheat Groats
                QuantityGrams = 150m
            },
            new RecipeProduct
            {
                RecipeId = recipes[18].RecipeId,
                ProductId = products[12].ProductId, // Tofu
                QuantityGrams = 120m
            },
            new RecipeProduct
            {
                RecipeId = recipes[18].RecipeId,
                ProductId = products[2].ProductId, // Broccoli
                QuantityGrams = 80m
            },
            new RecipeProduct
            {
                RecipeId = recipes[18].RecipeId,
                ProductId = products[24].ProductId, // Bell Pepper
                QuantityGrams = 50m
            },
            new RecipeProduct
            {
                RecipeId = recipes[19].RecipeId, // Lentil Salad with Vegetables
                ProductId = products[16].ProductId, // Lentils
                QuantityGrams = 120m
            },
            new RecipeProduct
            {
                RecipeId = recipes[19].RecipeId,
                ProductId = products[24].ProductId, // Bell Pepper
                QuantityGrams = 60m
            },
            new RecipeProduct
            {
                RecipeId = recipes[19].RecipeId,
                ProductId = products[25].ProductId, // Tomato
                QuantityGrams = 50m
            },
            new RecipeProduct
            {
                RecipeId = recipes[19].RecipeId,
                ProductId = products[26].ProductId, // Zucchini
                QuantityGrams = 50m
            },
            new RecipeProduct
            {
                RecipeId = recipes[20].RecipeId, // Tofu and Quinoa Power Bowl
                ProductId = products[20].ProductId, // Quinoa
                QuantityGrams = 120m
            },
            new RecipeProduct
            {
                RecipeId = recipes[20].RecipeId,
                ProductId = products[12].ProductId, // Tofu
                QuantityGrams = 120m
            },
            new RecipeProduct
            {
                RecipeId = recipes[20].RecipeId,
                ProductId = products[18].ProductId, // Spinach
                QuantityGrams = 50m
            },
            new RecipeProduct
            {
                RecipeId = recipes[20].RecipeId,
                ProductId = products[24].ProductId, // Bell Pepper
                QuantityGrams = 50m
            },
            new RecipeProduct
            {
                RecipeId = recipes[21].RecipeId, // Chickpea and Cauliflower Stew
                ProductId = products[17].ProductId, // Chickpeas
                QuantityGrams = 150m
            },
            new RecipeProduct
            {
                RecipeId = recipes[21].RecipeId,
                ProductId = products[27].ProductId, // Cauliflower
                QuantityGrams = 100m
            },
            new RecipeProduct
            {
                RecipeId = recipes[21].RecipeId,
                ProductId = products[25].ProductId, // Tomato
                QuantityGrams = 50m
            },
            new RecipeProduct
            {
                RecipeId = recipes[21].RecipeId,
                ProductId = products[26].ProductId, // Zucchini
                QuantityGrams = 50m
            },
            new RecipeProduct
            {
                RecipeId = recipes[22].RecipeId, // Apple and Blueberry Snack
                ProductId = products[13].ProductId, // Apple
                QuantityGrams = 150m
            },
            new RecipeProduct
            {
                RecipeId = recipes[22].RecipeId,
                ProductId = products[15].ProductId, // Blueberries
                QuantityGrams = 50m
            }
        };
    }

    private static List<MealRecipe> CreateMealRecipes(List<Meal> meals, List<Recipe> recipes)
    {
        return new List<MealRecipe>
        {
            // Користувач 1 (meals[0-4]): Vegetarian + Diabetes + Eggs allergen
            // Breakfast: Quinoa Breakfast Bowl (recipes[14]) - підходить
            new MealRecipe
            {
                MealId = meals[0].MealId,
                RecipeId = recipes[14].RecipeId
            },
            // Snack 1: Apple and Blueberry Snack (recipes[22]) - підходить
            new MealRecipe
            {
                MealId = meals[1].MealId,
                RecipeId = recipes[22].RecipeId
            },
            // Lunch: Tofu and Quinoa Power Bowl (recipes[20]) - підходить
            new MealRecipe
            {
                MealId = meals[2].MealId,
                RecipeId = recipes[20].RecipeId
            },
            // Snack 2: Lentil Salad with Vegetables (recipes[19]) - підходить
            new MealRecipe
            {
                MealId = meals[3].MealId,
                RecipeId = recipes[19].RecipeId
            },
            // Dinner: Chickpea and Spinach Curry (recipes[17]) - підходить
            new MealRecipe
            {
                MealId = meals[4].MealId,
                RecipeId = recipes[17].RecipeId
            },
            // Користувач 2 (meals[5-6]): Pescatarian + Hypertension + Milk allergen
            new MealRecipe
            {
                MealId = meals[5].MealId,
                RecipeId = recipes[2].RecipeId
            },
            new MealRecipe
            {
                MealId = meals[6].MealId,
                RecipeId = recipes[1].RecipeId
            },
            // Користувач 3 (meals[7-12]): Admin - без обмежень
            new MealRecipe
            {
                MealId = meals[7].MealId,
                RecipeId = recipes[2].RecipeId
            },
            new MealRecipe
            {
                MealId = meals[10].MealId,
                RecipeId = recipes[0].RecipeId
            },
            new MealRecipe
            {
                MealId = meals[11].MealId,
                RecipeId = recipes[1].RecipeId
            }
        };
    }

    private static List<Recommendation> CreateRecommendations(List<Meal> meals)
    {
        return new List<Recommendation>
        {
            new Recommendation
            {
                MealInstanceId = meals[0].MealId,
                RecommendationType = RecommendationType.Nutrition,
                RecommendationPayload = "Consider adding more protein to your breakfast",
                RecommendationStatus = RecommendationStatus.New,
                RecommendationCreatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new Recommendation
            {
                MealInstanceId = meals[2].MealId,
                RecommendationType = RecommendationType.Nutrition,
                RecommendationPayload = "Great choice for lunch! Keep it up.",
                RecommendationStatus = RecommendationStatus.Read,
                RecommendationCreatedAt = DateTime.UtcNow.AddHours(-5)
            },
            new Recommendation
            {
                MealInstanceId = meals[4].MealId,
                RecommendationType = RecommendationType.Health,
                RecommendationPayload = "Dinner looks balanced. Good job!",
                RecommendationStatus = RecommendationStatus.Applied,
                RecommendationCreatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Recommendation
            {
                MealInstanceId = null,
                RecommendationType = RecommendationType.General,
                RecommendationPayload = "Remember to stay hydrated throughout the day",
                RecommendationStatus = RecommendationStatus.New,
                RecommendationCreatedAt = DateTime.UtcNow
            }
        };
    }

    private static List<Device> CreateDevices(List<User> users)
    {
        return new List<Device>
        {
            new Device
            {
                UserId = users[0].UserId,
                DeviceType = DeviceType.FitnessTracker,
                ConnectionType = ConnectionType.Bluetooth,
                LastSeen = DateTime.UtcNow.AddHours(-1),
                Serial = "FT-001-2024"
            },
            new Device
            {
                UserId = users[0].UserId,
                DeviceType = DeviceType.FitnessTracker,
                ConnectionType = ConnectionType.WiFi,
                LastSeen = DateTime.UtcNow.AddMinutes(-30),
                Serial = "FT-002-2024"
            },
            new Device
            {
                UserId = users[1].UserId,
                DeviceType = DeviceType.FitnessTracker,
                ConnectionType = ConnectionType.Bluetooth,
                LastSeen = DateTime.UtcNow.AddHours(-2),
                Serial = "FT-003-2024"
            }
        };
    }

    private static List<TelemetrySample> CreateTelemetrySamples(List<Device> devices)
    {
        var samples = new List<TelemetrySample>();
        
        // Для користувача 1 (devices[0] та devices[1]) не генеруємо телеметрії
        // Всі дані будуть генеруватися тільки через IoT клієнт
        
        // Генеруємо телеметрії тільки для інших користувачів (devices[2])
        if (devices.Count > 2)
        {
            var now = DateTime.UtcNow;
            
            for (int i = 0; i < 5; i++)
            {
                samples.Add(new TelemetrySample
                {
                    DeviceId = devices[2].DeviceId,
                    Timestamp = now.AddHours(-i),
                    TelemetryType = TelemetryType.HeartRate,
                    TelemetryValue = 70 + (i * 1) + new Random().Next(-5, 5)
                });
            }
        }

        return samples;
    }

    private static List<SleepRecord> CreateSleepRecords(List<Device> devices)
    {
        var records = new List<SleepRecord>();
        var today = DateTime.UtcNow.Date;

        for (int i = 0; i < 7; i++)
        {
            var date = today.AddDays(-i);
            records.Add(new SleepRecord
            {
                DeviceId = devices[0].DeviceId,
                Date = date,
                TotalSleepMinutes = 420 + new Random().Next(-30, 30),
                DeepSleepMinutes = 120 + new Random().Next(-20, 20),
                LightSleepMinutes = 250 + new Random().Next(-30, 30),
                AwakeMinutes = 50 + new Random().Next(-10, 10),
                SleepQuality = 75m + new Random().Next(-10, 10),
                StartTime = date.AddHours(22).AddMinutes(new Random().Next(-30, 30)),
                EndTime = date.AddDays(1).AddHours(6).AddMinutes(new Random().Next(-30, 30))
            });
        }

        for (int i = 0; i < 5; i++)
        {
            var date = today.AddDays(-i);
            records.Add(new SleepRecord
            {
                DeviceId = devices[1].DeviceId,
                Date = date,
                TotalSleepMinutes = 480 + new Random().Next(-40, 40),
                DeepSleepMinutes = 150 + new Random().Next(-25, 25),
                LightSleepMinutes = 280 + new Random().Next(-40, 40),
                AwakeMinutes = 50 + new Random().Next(-15, 15),
                SleepQuality = 80m + new Random().Next(-10, 10),
                StartTime = date.AddHours(23).AddMinutes(new Random().Next(-30, 30)),
                EndTime = date.AddDays(1).AddHours(7).AddMinutes(new Random().Next(-30, 30))
            });
        }

        for (int i = 0; i < 3; i++)
        {
            var date = today.AddDays(-i);
            records.Add(new SleepRecord
            {
                DeviceId = devices[2].DeviceId,
                Date = date,
                TotalSleepMinutes = 450 + new Random().Next(-35, 35),
                DeepSleepMinutes = 135 + new Random().Next(-20, 20),
                LightSleepMinutes = 270 + new Random().Next(-35, 35),
                AwakeMinutes = 45 + new Random().Next(-10, 10),
                SleepQuality = 78m + new Random().Next(-8, 8),
                StartTime = date.AddHours(22).AddMinutes(30).AddMinutes(new Random().Next(-30, 30)),
                EndTime = date.AddDays(1).AddHours(6).AddMinutes(30).AddMinutes(new Random().Next(-30, 30))
            });
        }

        return records;
    }

    private static List<TrainingSession> CreateTrainingSessions(List<Device> devices)
    {
        var sessions = new List<TrainingSession>();
        var now = DateTime.UtcNow;

        for (int i = 0; i < 5; i++)
        {
            var startTime = now.AddDays(-i).AddHours(8);
            sessions.Add(new TrainingSession
            {
                DeviceId = devices[0].DeviceId,
                StartTime = startTime,
                EndTime = startTime.AddMinutes(45),
                Type = i % 2 == 0 ? TrainingType.Cardio : TrainingType.Strength,
                Intensity = i % 3 == 0 ? Intensity.Moderate : Intensity.High,
                DurationInMin = 45,
                CaloriesEstimated = 300m + (i * 20) + new Random().Next(-30, 30)
            });
        }

        for (int i = 0; i < 7; i++)
        {
            var startTime = now.AddDays(-i).AddHours(7);
            sessions.Add(new TrainingSession
            {
                DeviceId = devices[1].DeviceId,
                StartTime = startTime,
                EndTime = startTime.AddMinutes(60),
                Type = i % 3 == 0 ? TrainingType.Cardio : (i % 3 == 1 ? TrainingType.Strength : TrainingType.Mixed),
                Intensity = i % 4 == 0 ? Intensity.Low : (i % 4 == 1 ? Intensity.Moderate : Intensity.High),
                DurationInMin = 60,
                CaloriesEstimated = 400m + (i * 15) + new Random().Next(-40, 40)
            });
        }

        for (int i = 0; i < 3; i++)
        {
            var startTime = now.AddDays(-i).AddHours(18);
            sessions.Add(new TrainingSession
            {
                DeviceId = devices[2].DeviceId,
                StartTime = startTime,
                EndTime = startTime.AddMinutes(30),
                Type = TrainingType.Flexibility,
                Intensity = Intensity.Low,
                DurationInMin = 30,
                CaloriesEstimated = 150m + new Random().Next(-20, 20)
            });
        }

        return sessions;
    }

    private static string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}

