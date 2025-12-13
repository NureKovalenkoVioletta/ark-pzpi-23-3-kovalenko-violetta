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
        if (await context.Users.AnyAsync() && !forceRecreate)
        {
            return;
        }

        if (forceRecreate)
        {
            context.TrainingSessions.RemoveRange(context.TrainingSessions);
            context.SleepRecords.RemoveRange(context.SleepRecords);
            context.TelemetrySamples.RemoveRange(context.TelemetrySamples);
            context.Devices.RemoveRange(context.Devices);
            context.Recommendations.RemoveRange(context.Recommendations);
            context.MealRecipes.RemoveRange(context.MealRecipes);
            context.RecipeProducts.RemoveRange(context.RecipeProducts);
            context.Meals.RemoveRange(context.Meals);
            context.DailyDietPlans.RemoveRange(context.DailyDietPlans);
            context.Recipes.RemoveRange(context.Recipes);
            context.Products.RemoveRange(context.Products);
            context.TemplateDietPlans.RemoveRange(context.TemplateDietPlans);
            context.UserProfiles.RemoveRange(context.UserProfiles);
            context.Users.RemoveRange(context.Users);
            await context.SaveChangesAsync();
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

        var telemetrySamples = CreateTelemetrySamples(devices);
        await context.TelemetrySamples.AddRangeAsync(telemetrySamples);
        await context.SaveChangesAsync();

        var sleepRecords = CreateSleepRecords(devices);
        await context.SleepRecords.AddRangeAsync(sleepRecords);
        await context.SaveChangesAsync();

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
                MedicalConditions = null,
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
                MedicalConditions = "None",
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
                MedicalConditions = null,
                PreferredUnits = PreferredUnits.Imperial,
                BirthDate = new DateTime(1985, 3, 10)
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
                Restriction = null,
                Allergens = null,
                Unit = "g"
            },
            new Product
            {
                ProductName = "Brown Rice",
                CaloriesPer100g = 111m,
                ProteinPer100g = 2.6m,
                FatPer100g = 0.9m,
                CarbsPer100g = 23m,
                Restriction = null,
                Allergens = null,
                Unit = "g"
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
                Unit = "g"
            },
            new Product
            {
                ProductName = "Salmon",
                CaloriesPer100g = 208m,
                ProteinPer100g = 20m,
                FatPer100g = 12m,
                CarbsPer100g = 0m,
                Restriction = null,
                Allergens = "Fish",
                Unit = "g"
            },
            new Product
            {
                ProductName = "Eggs",
                CaloriesPer100g = 155m,
                ProteinPer100g = 13m,
                FatPer100g = 11m,
                CarbsPer100g = 1.1m,
                Restriction = null,
                Allergens = "Eggs",
                Unit = "piece"
            },
            new Product
            {
                ProductName = "Oatmeal",
                CaloriesPer100g = 389m,
                ProteinPer100g = 17m,
                FatPer100g = 7m,
                CarbsPer100g = 66m,
                Restriction = null,
                Allergens = "Gluten",
                Unit = "g"
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
                RecipeInstructions = "1. Season chicken breast with salt and pepper. 2. Grill for 6-7 minutes per side. 3. Cook brown rice according to package. 4. Serve together.",
                RecipeCaloriesPerPortion = 450m,
                RecipeFatPerPortion = 8m,
                RecipeCarbsPerPortion = 45m,
                RecipeProteinPerPortion = 45m,
                RecipeProductsGrams = 300m
            },
            new Recipe
            {
                RecipeName = "Salmon with Broccoli",
                RecipeInstructions = "1. Season salmon with lemon and herbs. 2. Bake at 200°C for 15 minutes. 3. Steam broccoli for 5 minutes. 4. Serve together.",
                RecipeCaloriesPerPortion = 380m,
                RecipeFatPerPortion = 18m,
                RecipeCarbsPerPortion = 12m,
                RecipeProteinPerPortion = 35m,
                RecipeProductsGrams = 250m
            },
            new Recipe
            {
                RecipeName = "Scrambled Eggs",
                RecipeInstructions = "1. Beat 3 eggs. 2. Heat pan with butter. 3. Cook eggs until fluffy. 4. Season with salt and pepper.",
                RecipeCaloriesPerPortion = 280m,
                RecipeFatPerPortion = 20m,
                RecipeCarbsPerPortion = 2m,
                RecipeProteinPerPortion = 20m,
                RecipeProductsGrams = 150m
            },
            new Recipe
            {
                RecipeName = "Oatmeal Bowl",
                RecipeInstructions = "1. Cook 50g oatmeal with water. 2. Add fruits and nuts. 3. Serve warm.",
                RecipeCaloriesPerPortion = 250m,
                RecipeFatPerPortion = 5m,
                RecipeCarbsPerPortion = 45m,
                RecipeProteinPerPortion = 8m,
                RecipeProductsGrams = 100m
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
            }
        };
    }

    private static List<MealRecipe> CreateMealRecipes(List<Meal> meals, List<Recipe> recipes)
    {
        return new List<MealRecipe>
        {
            new MealRecipe
            {
                MealId = meals[0].MealId,
                RecipeId = recipes[3].RecipeId
            },
            new MealRecipe
            {
                MealId = meals[5].MealId,
                RecipeId = recipes[2].RecipeId
            },
            new MealRecipe
            {
                MealId = meals[7].MealId,
                RecipeId = recipes[2].RecipeId
            },
            new MealRecipe
            {
                MealId = meals[2].MealId,
                RecipeId = recipes[0].RecipeId
            },
            new MealRecipe
            {
                MealId = meals[6].MealId,
                RecipeId = recipes[1].RecipeId
            },
            new MealRecipe
            {
                MealId = meals[10].MealId,
                RecipeId = recipes[0].RecipeId
            },
            new MealRecipe
            {
                MealId = meals[4].MealId,
                RecipeId = recipes[1].RecipeId
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
        var now = DateTime.UtcNow;

        for (int i = 0; i < 10; i++)
        {
            samples.Add(new TelemetrySample
            {
                DeviceId = devices[0].DeviceId,
                Timestamp = now.AddHours(-i),
                TelemetryType = TelemetryType.HeartRate,
                TelemetryValue = 65 + (i * 2) + new Random().Next(-5, 5)
            });
        }

        for (int i = 0; i < 5; i++)
        {
            samples.Add(new TelemetrySample
            {
                DeviceId = devices[0].DeviceId,
                Timestamp = now.AddHours(-i * 2),
                TelemetryType = TelemetryType.Steps,
                TelemetryValue = 1000 + (i * 500) + new Random().Next(-100, 100)
            });
        }

        for (int i = 0; i < 3; i++)
        {
            samples.Add(new TelemetrySample
            {
                DeviceId = devices[0].DeviceId,
                Timestamp = now.AddHours(-i * 4),
                TelemetryType = TelemetryType.BloodPressure,
                TelemetryValue = 120 + new Random().Next(-10, 10)
            });
        }

        for (int i = 0; i < 5; i++)
        {
            samples.Add(new TelemetrySample
            {
                DeviceId = devices[1].DeviceId,
                Timestamp = now.AddHours(-i),
                TelemetryType = TelemetryType.HeartRate,
                TelemetryValue = 70 + (i * 1) + new Random().Next(-5, 5)
            });
        }

        for (int i = 0; i < 8; i++)
        {
            samples.Add(new TelemetrySample
            {
                DeviceId = devices[2].DeviceId,
                Timestamp = now.AddHours(-i),
                TelemetryType = TelemetryType.Steps,
                TelemetryValue = 800 + (i * 300) + new Random().Next(-50, 50)
            });
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

