using Microsoft.EntityFrameworkCore;
using FitnessProject.Entities;

namespace FitnessProject.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<User> Users { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<TemplateDietPlan> TemplateDietPlans { get; set; }
    public DbSet<DailyDietPlan> DailyDietPlans { get; set; }
    public DbSet<Meal> Meals { get; set; }
    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<MealRecipe> MealRecipes { get; set; }
    public DbSet<RecipeProduct> RecipeProducts { get; set; }
    public DbSet<Recommendation> Recommendations { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<TelemetrySample> TelemetrySamples { get; set; }
    public DbSet<SleepRecord> SleepRecords { get; set; }
    public DbSet<TrainingSession> TrainingSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).HasColumnName("user_id").ValueGeneratedOnAdd();
            entity.Property(e => e.Email).HasColumnName("email").IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Locale).HasColumnName("locale").HasMaxLength(10);
            entity.Property(e => e.Role).HasColumnName("role").HasConversion<int>();
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.ToTable("UserProfiles");
            entity.HasKey(e => e.ProfileId);
            entity.Property(e => e.ProfileId).HasColumnName("profile_id").ValueGeneratedOnAdd();
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.FirstName).HasColumnName("first_name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).HasColumnName("last_name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Sex).HasColumnName("sex").HasConversion<int>();
            entity.Property(e => e.HeightCm).HasColumnName("height_cm").HasColumnType("decimal(5,2)");
            entity.Property(e => e.CurrentWeightKg).HasColumnName("current_weight_kg").HasColumnType("decimal(5,2)");
            entity.Property(e => e.ActivityLevel).HasColumnName("activity_level").HasConversion<int>();
            entity.Property(e => e.GoalType).HasColumnName("goal_type").HasConversion<int>();
            entity.Property(e => e.MedicalConditions).HasColumnName("medical_conditions");
            entity.Property(e => e.PreferredUnits).HasColumnName("preferred_units").HasConversion<int>();
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");

            entity.HasOne(e => e.User)
                .WithOne(u => u.UserProfile)
                .HasForeignKey<UserProfile>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TemplateDietPlan>(entity =>
        {
            entity.ToTable("TemplateDietPlans");
            entity.HasKey(e => e.TemplateDietPlanId);
            entity.Property(e => e.TemplateDietPlanId).HasColumnName("template_diet_plan_id").ValueGeneratedOnAdd();
            entity.Property(e => e.TemplateName).HasColumnName("template_name").IsRequired().HasMaxLength(200);
            entity.Property(e => e.TemplateDescription).HasColumnName("template_description");
            entity.Property(e => e.TemplateCaloriesMin).HasColumnName("template_calories_min").HasColumnType("decimal(10,2)");
            entity.Property(e => e.TemplateCaloriesMax).HasColumnName("template_calories_max").HasColumnType("decimal(10,2)");
            entity.Property(e => e.TemplateProteinMin).HasColumnName("template_protein_min").HasColumnType("decimal(10,2)");
            entity.Property(e => e.TemplateProteinMax).HasColumnName("template_protein_max").HasColumnType("decimal(10,2)");
            entity.Property(e => e.TemplateFatMin).HasColumnName("template_fat_min").HasColumnType("decimal(10,2)");
            entity.Property(e => e.TemplateFatMax).HasColumnName("template_fat_max").HasColumnType("decimal(10,2)");
            entity.Property(e => e.TemplateCarbsMin).HasColumnName("template_carbs_min").HasColumnType("decimal(10,2)");
            entity.Property(e => e.TemplateCarbsMax).HasColumnName("template_carbs_max").HasColumnType("decimal(10,2)");
            entity.Property(e => e.TemplateNumberOfMeals).HasColumnName("template_number_of_meals");
            entity.Property(e => e.TemplateStatus).HasColumnName("template_status").HasConversion<int>();
            entity.Property(e => e.TemplateCreatedAt).HasColumnName("template_created_at");
        });

        modelBuilder.Entity<DailyDietPlan>(entity =>
        {
            entity.ToTable("DailyDietPlans");
            entity.HasKey(e => e.DailyDietPlanId);
            entity.Property(e => e.DailyDietPlanId).HasColumnName("daily_diet_plan_id").ValueGeneratedOnAdd();
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.TemplateDietPlanId).HasColumnName("template_diet_plan_id");
            entity.Property(e => e.DailyDietPlanName).HasColumnName("daily_diet_plan_name").IsRequired().HasMaxLength(200);
            entity.Property(e => e.DailyPlanDescription).HasColumnName("daily_plan_description");
            entity.Property(e => e.DailyPlanCalories).HasColumnName("daily_plan_calories").HasColumnType("decimal(10,2)");
            entity.Property(e => e.DailyPlanFat).HasColumnName("daily_plan_fat").HasColumnType("decimal(10,2)");
            entity.Property(e => e.DailyPlanCarbs).HasColumnName("daily_plan_carbs").HasColumnType("decimal(10,2)");
            entity.Property(e => e.DailyPlanProtein).HasColumnName("daily_plan_protein").HasColumnType("decimal(10,2)");
            entity.Property(e => e.DailyPlanNumberOfMeals).HasColumnName("daily_plan_number_of_meals");
            entity.Property(e => e.DailyPlanStatus).HasColumnName("daily_plan_status").HasConversion<int>();
            entity.Property(e => e.DailyPlanCreatedAt).HasColumnName("daily_plan_created_at");

            entity.HasOne(e => e.User)
                .WithMany(u => u.DailyDietPlans)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.TemplateDietPlan)
                .WithMany(t => t.DailyDietPlans)
                .HasForeignKey(e => e.TemplateDietPlanId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Meal>(entity =>
        {
            entity.ToTable("Meals");
            entity.HasKey(e => e.MealId);
            entity.Property(e => e.MealId).HasColumnName("meal_id").ValueGeneratedOnAdd();
            entity.Property(e => e.DailyDietPlanId).HasColumnName("daily_diet_plan_id");
            entity.Property(e => e.MealTime).HasColumnName("meal_time").HasConversion<int>();
            entity.Property(e => e.MealOrder).HasColumnName("meal_order");
            entity.Property(e => e.MealTargetCalories).HasColumnName("meal_target_calories").HasColumnType("decimal(10,2)");
            entity.Property(e => e.MealTargetFat).HasColumnName("meal_target_fat").HasColumnType("decimal(10,2)");
            entity.Property(e => e.MealTargetCarbs).HasColumnName("meal_target_carbs").HasColumnType("decimal(10,2)");
            entity.Property(e => e.MealTargetProtein).HasColumnName("meal_target_protein").HasColumnType("decimal(10,2)");

            entity.HasOne(e => e.DailyDietPlan)
                .WithMany(d => d.Meals)
                .HasForeignKey(e => e.DailyDietPlanId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.ToTable("Recipes");
            entity.HasKey(e => e.RecipeId);
            entity.Property(e => e.RecipeId).HasColumnName("recipe_id").ValueGeneratedOnAdd();
            entity.Property(e => e.RecipeName).HasColumnName("recipe_name").IsRequired().HasMaxLength(200);
            entity.Property(e => e.RecipeInstructions).HasColumnName("recipe_instructions").IsRequired();
            entity.Property(e => e.RecipeCaloriesPerPortion).HasColumnName("recipe_calories_per_portion").HasColumnType("decimal(10,2)");
            entity.Property(e => e.RecipeFatPerPortion).HasColumnName("recipe_fat_per_portion").HasColumnType("decimal(10,2)");
            entity.Property(e => e.RecipeCarbsPerPortion).HasColumnName("recipe_carbs_per_portion").HasColumnType("decimal(10,2)");
            entity.Property(e => e.RecipeProteinPerPortion).HasColumnName("recipe_protein_per_portion").HasColumnType("decimal(10,2)");
            entity.Property(e => e.RecipeProductsGrams).HasColumnName("recipe_products_grams").HasColumnType("decimal(10,2)");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(e => e.ProductId);
            entity.Property(e => e.ProductId).HasColumnName("product_id").ValueGeneratedOnAdd();
            entity.Property(e => e.ProductName).HasColumnName("product_name").IsRequired().HasMaxLength(200);
            entity.Property(e => e.CaloriesPer100g).HasColumnName("calories_per_100g").HasColumnType("decimal(10,2)");
            entity.Property(e => e.ProteinPer100g).HasColumnName("protein_per_100g").HasColumnType("decimal(10,2)");
            entity.Property(e => e.FatPer100g).HasColumnName("fat_per_100g").HasColumnType("decimal(10,2)");
            entity.Property(e => e.CarbsPer100g).HasColumnName("carbs_per_100g").HasColumnType("decimal(10,2)");
            entity.Property(e => e.Restriction).HasColumnName("restriction");
            entity.Property(e => e.Allergens).HasColumnName("allergens");
            entity.Property(e => e.Unit).HasColumnName("unit").HasMaxLength(50);
        });

        modelBuilder.Entity<MealRecipe>(entity =>
        {
            entity.ToTable("MealRecipes");
            entity.HasKey(e => new { e.MealId, e.RecipeId });
            entity.Property(e => e.MealId).HasColumnName("meal_id");
            entity.Property(e => e.RecipeId).HasColumnName("recipe_id");
            entity.Property(e => e.PortionsMetadata).HasColumnName("portions_metadata");

            entity.HasOne(e => e.Meal)
                .WithMany(m => m.MealRecipes)
                .HasForeignKey(e => e.MealId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Recipe)
                .WithMany(r => r.MealRecipes)
                .HasForeignKey(e => e.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RecipeProduct>(entity =>
        {
            entity.ToTable("RecipeProducts");
            entity.HasKey(e => new { e.RecipeId, e.ProductId });
            entity.Property(e => e.RecipeId).HasColumnName("recipe_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.QuantityGrams).HasColumnName("quantity_grams").HasColumnType("decimal(10,2)");

            entity.HasOne(e => e.Recipe)
                .WithMany(r => r.RecipeProducts)
                .HasForeignKey(e => e.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.RecipeProducts)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Recommendation>(entity =>
        {
            entity.ToTable("Recommendations");
            entity.HasKey(e => e.RecommendationId);
            entity.Property(e => e.RecommendationId).HasColumnName("recommendation_id").ValueGeneratedOnAdd();
            entity.Property(e => e.MealInstanceId).HasColumnName("meal_instance_id");
            entity.Property(e => e.RecommendationCreatedAt).HasColumnName("recommendation_created_at");
            entity.Property(e => e.RecommendationType).HasColumnName("recommendation_type").HasConversion<int>();
            entity.Property(e => e.RecommendationPayload).HasColumnName("recommendation_payload");
            entity.Property(e => e.RecommendationStatus).HasColumnName("recommendation_status").HasConversion<int>();

            entity.HasOne(e => e.Meal)
                .WithMany(m => m.Recommendations)
                .HasForeignKey(e => e.MealInstanceId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.ToTable("Devices");
            entity.HasKey(e => e.DeviceId);
            entity.Property(e => e.DeviceId).HasColumnName("device_id").ValueGeneratedOnAdd();
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.DeviceType).HasColumnName("device_type").HasConversion<int>();
            entity.Property(e => e.ConnectionType).HasColumnName("connection_type").HasConversion<int>();
            entity.Property(e => e.LastSeen).HasColumnName("last_seen");
            entity.Property(e => e.Serial).HasColumnName("serial").HasMaxLength(100);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Devices)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TelemetrySample>(entity =>
        {
            entity.ToTable("TelemetrySamples");
            entity.HasKey(e => e.SampleId);
            entity.Property(e => e.SampleId).HasColumnName("sample_id").ValueGeneratedOnAdd();
            entity.Property(e => e.DeviceId).HasColumnName("device_id");
            entity.Property(e => e.Timestamp).HasColumnName("timestamp");
            entity.Property(e => e.TelemetryType).HasColumnName("telemetry_type").HasConversion<int>();
            entity.Property(e => e.TelemetryValue).HasColumnName("telemetry_value").HasColumnType("decimal(10,2)");

            entity.HasOne(e => e.Device)
                .WithMany(d => d.TelemetrySamples)
                .HasForeignKey(e => e.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SleepRecord>(entity =>
        {
            entity.ToTable("SleepRecords");
            entity.HasKey(e => e.SleepId);
            entity.Property(e => e.SleepId).HasColumnName("sleep_id").ValueGeneratedOnAdd();
            entity.Property(e => e.DeviceId).HasColumnName("device_id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.TotalSleepMinutes).HasColumnName("total_sleep_minutes");
            entity.Property(e => e.DeepSleepMinutes).HasColumnName("deep_sleep_minutes");
            entity.Property(e => e.LightSleepMinutes).HasColumnName("light_sleep_minutes");
            entity.Property(e => e.AwakeMinutes).HasColumnName("awake_minutes");
            entity.Property(e => e.SleepQuality).HasColumnName("sleep_quality").HasColumnType("decimal(5,2)");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.EndTime).HasColumnName("end_time");

            entity.HasOne(e => e.Device)
                .WithMany(d => d.SleepRecords)
                .HasForeignKey(e => e.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TrainingSession>(entity =>
        {
            entity.ToTable("TrainingSessions");
            entity.HasKey(e => e.SessionId);
            entity.Property(e => e.SessionId).HasColumnName("session_id").ValueGeneratedOnAdd();
            entity.Property(e => e.DeviceId).HasColumnName("device_id");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.Type).HasColumnName("type").HasConversion<int>();
            entity.Property(e => e.Intensity).HasColumnName("intensity").HasConversion<int>();
            entity.Property(e => e.DurationInMin).HasColumnName("duration_in_min");
            entity.Property(e => e.CaloriesEstimated).HasColumnName("calories_estimated").HasColumnType("decimal(10,2)");

            entity.HasOne(e => e.Device)
                .WithMany(d => d.TrainingSessions)
                .HasForeignKey(e => e.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

