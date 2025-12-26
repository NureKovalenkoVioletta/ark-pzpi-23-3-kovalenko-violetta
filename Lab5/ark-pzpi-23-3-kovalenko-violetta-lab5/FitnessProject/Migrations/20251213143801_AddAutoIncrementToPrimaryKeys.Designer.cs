using System;
using FitnessProject.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace FitnessProject.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20251213143801_AddAutoIncrementToPrimaryKeys")]
    partial class AddAutoIncrementToPrimaryKeys
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("FitnessProject.Entities.DailyDietPlan", b =>
                {
                    b.Property<int>("DailyDietPlanId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("daily_diet_plan_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("DailyDietPlanId"));

                    b.Property<string>("DailyDietPlanName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)")
                        .HasColumnName("daily_diet_plan_name");

                    b.Property<decimal>("DailyPlanCalories")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("daily_plan_calories");

                    b.Property<decimal>("DailyPlanCarbs")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("daily_plan_carbs");

                    b.Property<DateTime>("DailyPlanCreatedAt")
                        .HasColumnType("datetime2")
                        .HasColumnName("daily_plan_created_at");

                    b.Property<string>("DailyPlanDescription")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("daily_plan_description");

                    b.Property<decimal>("DailyPlanFat")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("daily_plan_fat");

                    b.Property<int>("DailyPlanNumberOfMeals")
                        .HasColumnType("int")
                        .HasColumnName("daily_plan_number_of_meals");

                    b.Property<decimal>("DailyPlanProtein")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("daily_plan_protein");

                    b.Property<int>("DailyPlanStatus")
                        .HasColumnType("int")
                        .HasColumnName("daily_plan_status");

                    b.Property<int?>("TemplateDietPlanId")
                        .HasColumnType("int")
                        .HasColumnName("template_diet_plan_id");

                    b.Property<int>("UserId")
                        .HasColumnType("int")
                        .HasColumnName("user_id");

                    b.HasKey("DailyDietPlanId");

                    b.HasIndex("TemplateDietPlanId");

                    b.HasIndex("UserId");

                    b.ToTable("DailyDietPlans", (string)null);
                });

            modelBuilder.Entity("FitnessProject.Entities.Device", b =>
                {
                    b.Property<int>("DeviceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("device_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("DeviceId"));

                    b.Property<int>("ConnectionType")
                        .HasColumnType("int")
                        .HasColumnName("connection_type");

                    b.Property<int>("DeviceType")
                        .HasColumnType("int")
                        .HasColumnName("device_type");

                    b.Property<DateTime?>("LastSeen")
                        .HasColumnType("datetime2")
                        .HasColumnName("last_seen");

                    b.Property<string>("Serial")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("serial");

                    b.Property<int>("UserId")
                        .HasColumnType("int")
                        .HasColumnName("user_id");

                    b.HasKey("DeviceId");

                    b.HasIndex("UserId");

                    b.ToTable("Devices", (string)null);
                });

            modelBuilder.Entity("FitnessProject.Entities.Meal", b =>
                {
                    b.Property<int>("MealId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("meal_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MealId"));

                    b.Property<int>("DailyDietPlanId")
                        .HasColumnType("int")
                        .HasColumnName("daily_diet_plan_id");

                    b.Property<int>("MealOrder")
                        .HasColumnType("int")
                        .HasColumnName("meal_order");

                    b.Property<decimal>("MealTargetCalories")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("meal_target_calories");

                    b.Property<decimal>("MealTargetCarbs")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("meal_target_carbs");

                    b.Property<decimal>("MealTargetFat")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("meal_target_fat");

                    b.Property<decimal>("MealTargetProtein")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("meal_target_protein");

                    b.Property<int>("MealTime")
                        .HasColumnType("int")
                        .HasColumnName("meal_time");

                    b.HasKey("MealId");

                    b.HasIndex("DailyDietPlanId");

                    b.ToTable("Meals", (string)null);
                });

            modelBuilder.Entity("FitnessProject.Entities.MealRecipe", b =>
                {
                    b.Property<int>("MealId")
                        .HasColumnType("int")
                        .HasColumnName("meal_id");

                    b.Property<int>("RecipeId")
                        .HasColumnType("int")
                        .HasColumnName("recipe_id");

                    b.HasKey("MealId", "RecipeId");

                    b.HasIndex("RecipeId");

                    b.ToTable("MealRecipes", (string)null);
                });

            modelBuilder.Entity("FitnessProject.Entities.Product", b =>
                {
                    b.Property<int>("ProductId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("product_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ProductId"));

                    b.Property<string>("Allergens")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("allergens");

                    b.Property<decimal>("CaloriesPer100g")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("calories_per_100g");

                    b.Property<decimal>("CarbsPer100g")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("carbs_per_100g");

                    b.Property<decimal>("FatPer100g")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("fat_per_100g");

                    b.Property<string>("ProductName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)")
                        .HasColumnName("product_name");

                    b.Property<decimal>("ProteinPer100g")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("protein_per_100g");

                    b.Property<string>("Restriction")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("restriction");

                    b.Property<string>("Unit")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)")
                        .HasColumnName("unit");

                    b.HasKey("ProductId");

                    b.ToTable("Products", (string)null);
                });

            modelBuilder.Entity("FitnessProject.Entities.Recipe", b =>
                {
                    b.Property<int>("RecipeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("recipe_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("RecipeId"));

                    b.Property<decimal>("RecipeCaloriesPerPortion")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("recipe_calories_per_portion");

                    b.Property<decimal>("RecipeCarbsPerPortion")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("recipe_carbs_per_portion");

                    b.Property<decimal>("RecipeFatPerPortion")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("recipe_fat_per_portion");

                    b.Property<string>("RecipeInstructions")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("recipe_instructions");

                    b.Property<string>("RecipeName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)")
                        .HasColumnName("recipe_name");

                    b.Property<decimal>("RecipeProductsGrams")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("recipe_products_grams");

                    b.Property<decimal>("RecipeProteinPerPortion")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("recipe_protein_per_portion");

                    b.HasKey("RecipeId");

                    b.ToTable("Recipes", (string)null);
                });

            modelBuilder.Entity("FitnessProject.Entities.RecipeProduct", b =>
                {
                    b.Property<int>("RecipeId")
                        .HasColumnType("int")
                        .HasColumnName("recipe_id");

                    b.Property<int>("ProductId")
                        .HasColumnType("int")
                        .HasColumnName("product_id");

                    b.Property<decimal>("QuantityGrams")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("quantity_grams");

                    b.HasKey("RecipeId", "ProductId");

                    b.HasIndex("ProductId");

                    b.ToTable("RecipeProducts", (string)null);
                });

            modelBuilder.Entity("FitnessProject.Entities.Recommendation", b =>
                {
                    b.Property<int>("RecommendationId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("recommendation_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("RecommendationId"));

                    b.Property<int?>("MealInstanceId")
                        .HasColumnType("int")
                        .HasColumnName("meal_instance_id");

                    b.Property<DateTime>("RecommendationCreatedAt")
                        .HasColumnType("datetime2")
                        .HasColumnName("recommendation_created_at");

                    b.Property<string>("RecommendationPayload")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("recommendation_payload");

                    b.Property<int>("RecommendationStatus")
                        .HasColumnType("int")
                        .HasColumnName("recommendation_status");

                    b.Property<int>("RecommendationType")
                        .HasColumnType("int")
                        .HasColumnName("recommendation_type");

                    b.HasKey("RecommendationId");

                    b.HasIndex("MealInstanceId");

                    b.ToTable("Recommendations", (string)null);
                });

            modelBuilder.Entity("FitnessProject.Entities.SleepRecord", b =>
                {
                    b.Property<int>("SleepId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("sleep_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("SleepId"));

                    b.Property<int>("AwakeMinutes")
                        .HasColumnType("int")
                        .HasColumnName("awake_minutes");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2")
                        .HasColumnName("date");

                    b.Property<int>("DeepSleepMinutes")
                        .HasColumnType("int")
                        .HasColumnName("deep_sleep_minutes");

                    b.Property<int>("DeviceId")
                        .HasColumnType("int")
                        .HasColumnName("device_id");

                    b.Property<DateTime?>("EndTime")
                        .HasColumnType("datetime2")
                        .HasColumnName("end_time");

                    b.Property<int>("LightSleepMinutes")
                        .HasColumnType("int")
                        .HasColumnName("light_sleep_minutes");

                    b.Property<decimal?>("SleepQuality")
                        .HasColumnType("decimal(5,2)")
                        .HasColumnName("sleep_quality");

                    b.Property<DateTime?>("StartTime")
                        .HasColumnType("datetime2")
                        .HasColumnName("start_time");

                    b.Property<int>("TotalSleepMinutes")
                        .HasColumnType("int")
                        .HasColumnName("total_sleep_minutes");

                    b.HasKey("SleepId");

                    b.HasIndex("DeviceId");

                    b.ToTable("SleepRecords", (string)null);
                });

            modelBuilder.Entity("FitnessProject.Entities.TelemetrySample", b =>
                {
                    b.Property<int>("SampleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("sample_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("SampleId"));

                    b.Property<int>("DeviceId")
                        .HasColumnType("int")
                        .HasColumnName("device_id");

                    b.Property<int>("TelemetryType")
                        .HasColumnType("int")
                        .HasColumnName("telemetry_type");

                    b.Property<decimal>("TelemetryValue")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("telemetry_value");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("datetime2")
                        .HasColumnName("timestamp");

                    b.HasKey("SampleId");

                    b.HasIndex("DeviceId");

                    b.ToTable("TelemetrySamples", (string)null);
                });

            modelBuilder.Entity("FitnessProject.Entities.TemplateDietPlan", b =>
                {
                    b.Property<int>("TemplateDietPlanId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("template_diet_plan_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("TemplateDietPlanId"));

                    b.Property<decimal>("TemplateCaloriesMax")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("template_calories_max");

                    b.Property<decimal>("TemplateCaloriesMin")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("template_calories_min");

                    b.Property<decimal>("TemplateCarbsMax")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("template_carbs_max");

                    b.Property<decimal>("TemplateCarbsMin")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("template_carbs_min");

                    b.Property<DateTime>("TemplateCreatedAt")
                        .HasColumnType("datetime2")
                        .HasColumnName("template_created_at");

                    b.Property<string>("TemplateDescription")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("template_description");

                    b.Property<decimal>("TemplateFatMax")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("template_fat_max");

                    b.Property<decimal>("TemplateFatMin")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("template_fat_min");

                    b.Property<string>("TemplateName")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)")
                        .HasColumnName("template_name");

                    b.Property<int>("TemplateNumberOfMeals")
                        .HasColumnType("int")
                        .HasColumnName("template_number_of_meals");

                    b.Property<decimal>("TemplateProteinMax")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("template_protein_max");

                    b.Property<decimal>("TemplateProteinMin")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("template_protein_min");

                    b.Property<int>("TemplateStatus")
                        .HasColumnType("int")
                        .HasColumnName("template_status");

                    b.HasKey("TemplateDietPlanId");

                    b.ToTable("TemplateDietPlans", (string)null);
                });

            modelBuilder.Entity("FitnessProject.Entities.TrainingSession", b =>
                {
                    b.Property<int>("SessionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("session_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("SessionId"));

                    b.Property<decimal?>("CaloriesEstimated")
                        .HasColumnType("decimal(10,2)")
                        .HasColumnName("calories_estimated");

                    b.Property<int>("DeviceId")
                        .HasColumnType("int")
                        .HasColumnName("device_id");

                    b.Property<int>("DurationInMin")
                        .HasColumnType("int")
                        .HasColumnName("duration_in_min");

                    b.Property<DateTime?>("EndTime")
                        .HasColumnType("datetime2")
                        .HasColumnName("end_time");

                    b.Property<int>("Intensity")
                        .HasColumnType("int")
                        .HasColumnName("intensity");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("datetime2")
                        .HasColumnName("start_time");

                    b.Property<int>("Type")
                        .HasColumnType("int")
                        .HasColumnName("type");

                    b.HasKey("SessionId");

                    b.HasIndex("DeviceId");

                    b.ToTable("TrainingSessions", (string)null);
                });

            modelBuilder.Entity("FitnessProject.Entities.User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("user_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("UserId"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2")
                        .HasColumnName("created_at");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)")
                        .HasColumnName("email");

                    b.Property<string>("Locale")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)")
                        .HasColumnName("locale");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("password_hash");

                    b.Property<int>("Role")
                        .HasColumnType("int")
                        .HasColumnName("role");

                    b.HasKey("UserId");

                    b.ToTable("Users", (string)null);
                });

            modelBuilder.Entity("FitnessProject.Entities.UserProfile", b =>
                {
                    b.Property<int>("ProfileId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("profile_id");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ProfileId"));

                    b.Property<int>("ActivityLevel")
                        .HasColumnType("int")
                        .HasColumnName("activity_level");

                    b.Property<DateTime?>("BirthDate")
                        .HasColumnType("datetime2")
                        .HasColumnName("birth_date");

                    b.Property<decimal>("CurrentWeightKg")
                        .HasColumnType("decimal(5,2)")
                        .HasColumnName("current_weight_kg");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("first_name");

                    b.Property<decimal>("HeightCm")
                        .HasColumnType("decimal(5,2)")
                        .HasColumnName("height_cm");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)")
                        .HasColumnName("last_name");

                    b.Property<string>("MedicalConditions")
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("medical_conditions");

                    b.Property<int>("PreferredUnits")
                        .HasColumnType("int")
                        .HasColumnName("preferred_units");

                    b.Property<int>("Sex")
                        .HasColumnType("int")
                        .HasColumnName("sex");

                    b.Property<int>("UserId")
                        .HasColumnType("int")
                        .HasColumnName("user_id");

                    b.HasKey("ProfileId");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("UserProfiles", (string)null);
                });

            modelBuilder.Entity("FitnessProject.Entities.DailyDietPlan", b =>
                {
                    b.HasOne("FitnessProject.Entities.TemplateDietPlan", "TemplateDietPlan")
                        .WithMany("DailyDietPlans")
                        .HasForeignKey("TemplateDietPlanId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("FitnessProject.Entities.User", "User")
                        .WithMany("DailyDietPlans")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TemplateDietPlan");

                    b.Navigation("User");
                });

            modelBuilder.Entity("FitnessProject.Entities.Device", b =>
                {
                    b.HasOne("FitnessProject.Entities.User", "User")
                        .WithMany("Devices")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("FitnessProject.Entities.Meal", b =>
                {
                    b.HasOne("FitnessProject.Entities.DailyDietPlan", "DailyDietPlan")
                        .WithMany("Meals")
                        .HasForeignKey("DailyDietPlanId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DailyDietPlan");
                });

            modelBuilder.Entity("FitnessProject.Entities.MealRecipe", b =>
                {
                    b.HasOne("FitnessProject.Entities.Meal", "Meal")
                        .WithMany("MealRecipes")
                        .HasForeignKey("MealId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FitnessProject.Entities.Recipe", "Recipe")
                        .WithMany("MealRecipes")
                        .HasForeignKey("RecipeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Meal");

                    b.Navigation("Recipe");
                });

            modelBuilder.Entity("FitnessProject.Entities.RecipeProduct", b =>
                {
                    b.HasOne("FitnessProject.Entities.Product", "Product")
                        .WithMany("RecipeProducts")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FitnessProject.Entities.Recipe", "Recipe")
                        .WithMany("RecipeProducts")
                        .HasForeignKey("RecipeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Product");

                    b.Navigation("Recipe");
                });

            modelBuilder.Entity("FitnessProject.Entities.Recommendation", b =>
                {
                    b.HasOne("FitnessProject.Entities.Meal", "Meal")
                        .WithMany("Recommendations")
                        .HasForeignKey("MealInstanceId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Meal");
                });

            modelBuilder.Entity("FitnessProject.Entities.SleepRecord", b =>
                {
                    b.HasOne("FitnessProject.Entities.Device", "Device")
                        .WithMany("SleepRecords")
                        .HasForeignKey("DeviceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Device");
                });

            modelBuilder.Entity("FitnessProject.Entities.TelemetrySample", b =>
                {
                    b.HasOne("FitnessProject.Entities.Device", "Device")
                        .WithMany("TelemetrySamples")
                        .HasForeignKey("DeviceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Device");
                });

            modelBuilder.Entity("FitnessProject.Entities.TrainingSession", b =>
                {
                    b.HasOne("FitnessProject.Entities.Device", "Device")
                        .WithMany("TrainingSessions")
                        .HasForeignKey("DeviceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Device");
                });

            modelBuilder.Entity("FitnessProject.Entities.UserProfile", b =>
                {
                    b.HasOne("FitnessProject.Entities.User", "User")
                        .WithOne("UserProfile")
                        .HasForeignKey("FitnessProject.Entities.UserProfile", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("FitnessProject.Entities.DailyDietPlan", b =>
                {
                    b.Navigation("Meals");
                });

            modelBuilder.Entity("FitnessProject.Entities.Device", b =>
                {
                    b.Navigation("SleepRecords");

                    b.Navigation("TelemetrySamples");

                    b.Navigation("TrainingSessions");
                });

            modelBuilder.Entity("FitnessProject.Entities.Meal", b =>
                {
                    b.Navigation("MealRecipes");

                    b.Navigation("Recommendations");
                });

            modelBuilder.Entity("FitnessProject.Entities.Product", b =>
                {
                    b.Navigation("RecipeProducts");
                });

            modelBuilder.Entity("FitnessProject.Entities.Recipe", b =>
                {
                    b.Navigation("MealRecipes");

                    b.Navigation("RecipeProducts");
                });

            modelBuilder.Entity("FitnessProject.Entities.TemplateDietPlan", b =>
                {
                    b.Navigation("DailyDietPlans");
                });

            modelBuilder.Entity("FitnessProject.Entities.User", b =>
                {
                    b.Navigation("DailyDietPlans");

                    b.Navigation("Devices");

                    b.Navigation("UserProfile");
                });
#pragma warning restore 612, 618
        }
    }
}
