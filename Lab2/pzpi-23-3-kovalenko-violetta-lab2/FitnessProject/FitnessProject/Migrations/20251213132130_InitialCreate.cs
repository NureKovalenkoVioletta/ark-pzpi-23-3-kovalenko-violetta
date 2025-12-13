using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessProject.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    product_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    product_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    calories_per_100g = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    protein_per_100g = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    fat_per_100g = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    carbs_per_100g = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    restriction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    allergens = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.product_id);
                });

            migrationBuilder.CreateTable(
                name: "Recipes",
                columns: table => new
                {
                    recipe_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    recipe_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    recipe_instructions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    recipe_calories_per_portion = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    recipe_fat_per_portion = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    recipe_carbs_per_portion = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    recipe_protein_per_portion = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    recipe_products_grams = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipes", x => x.recipe_id);
                });

            migrationBuilder.CreateTable(
                name: "TemplateDietPlans",
                columns: table => new
                {
                    template_diet_plan_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    template_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    template_description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    template_calories_min = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    template_calories_max = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    template_protein_min = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    template_protein_max = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    template_fat_min = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    template_fat_max = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    template_carbs_min = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    template_carbs_max = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    template_number_of_meals = table.Column<int>(type: "int", nullable: false),
                    template_status = table.Column<int>(type: "int", nullable: false),
                    template_created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateDietPlans", x => x.template_diet_plan_id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    locale = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "RecipeProducts",
                columns: table => new
                {
                    recipe_id = table.Column<int>(type: "int", nullable: false),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    quantity_grams = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeProducts", x => new { x.recipe_id, x.product_id });
                    table.ForeignKey(
                        name: "FK_RecipeProducts_Products_product_id",
                        column: x => x.product_id,
                        principalTable: "Products",
                        principalColumn: "product_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecipeProducts_Recipes_recipe_id",
                        column: x => x.recipe_id,
                        principalTable: "Recipes",
                        principalColumn: "recipe_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyDietPlans",
                columns: table => new
                {
                    daily_diet_plan_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    template_diet_plan_id = table.Column<int>(type: "int", nullable: true),
                    daily_diet_plan_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    daily_plan_description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    daily_plan_calories = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    daily_plan_fat = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    daily_plan_carbs = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    daily_plan_protein = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    daily_plan_number_of_meals = table.Column<int>(type: "int", nullable: false),
                    daily_plan_status = table.Column<int>(type: "int", nullable: false),
                    daily_plan_created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyDietPlans", x => x.daily_diet_plan_id);
                    table.ForeignKey(
                        name: "FK_DailyDietPlans_TemplateDietPlans_template_diet_plan_id",
                        column: x => x.template_diet_plan_id,
                        principalTable: "TemplateDietPlans",
                        principalColumn: "template_diet_plan_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DailyDietPlans_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    device_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    device_type = table.Column<int>(type: "int", nullable: false),
                    connection_type = table.Column<int>(type: "int", nullable: false),
                    last_seen = table.Column<DateTime>(type: "datetime2", nullable: true),
                    serial = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.device_id);
                    table.ForeignKey(
                        name: "FK_Devices_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    profile_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    first_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    sex = table.Column<int>(type: "int", nullable: false),
                    height_cm = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    current_weight_kg = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    activity_level = table.Column<int>(type: "int", nullable: false),
                    medical_conditions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    preferred_units = table.Column<int>(type: "int", nullable: false),
                    birth_date = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.profile_id);
                    table.ForeignKey(
                        name: "FK_UserProfiles_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Meals",
                columns: table => new
                {
                    meal_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    daily_diet_plan_id = table.Column<int>(type: "int", nullable: false),
                    meal_time = table.Column<int>(type: "int", nullable: false),
                    meal_order = table.Column<int>(type: "int", nullable: false),
                    meal_target_calories = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    meal_target_fat = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    meal_target_carbs = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    meal_target_protein = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meals", x => x.meal_id);
                    table.ForeignKey(
                        name: "FK_Meals_DailyDietPlans_daily_diet_plan_id",
                        column: x => x.daily_diet_plan_id,
                        principalTable: "DailyDietPlans",
                        principalColumn: "daily_diet_plan_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SleepRecords",
                columns: table => new
                {
                    sleep_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    device_id = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    total_sleep_minutes = table.Column<int>(type: "int", nullable: false),
                    deep_sleep_minutes = table.Column<int>(type: "int", nullable: false),
                    light_sleep_minutes = table.Column<int>(type: "int", nullable: false),
                    awake_minutes = table.Column<int>(type: "int", nullable: false),
                    sleep_quality = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    start_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    end_time = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SleepRecords", x => x.sleep_id);
                    table.ForeignKey(
                        name: "FK_SleepRecords_Devices_device_id",
                        column: x => x.device_id,
                        principalTable: "Devices",
                        principalColumn: "device_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TelemetrySamples",
                columns: table => new
                {
                    sample_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    device_id = table.Column<int>(type: "int", nullable: false),
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    telemetry_type = table.Column<int>(type: "int", nullable: false),
                    telemetry_value = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelemetrySamples", x => x.sample_id);
                    table.ForeignKey(
                        name: "FK_TelemetrySamples_Devices_device_id",
                        column: x => x.device_id,
                        principalTable: "Devices",
                        principalColumn: "device_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingSessions",
                columns: table => new
                {
                    session_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    device_id = table.Column<int>(type: "int", nullable: false),
                    start_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    end_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    type = table.Column<int>(type: "int", nullable: false),
                    intensity = table.Column<int>(type: "int", nullable: false),
                    duration_in_min = table.Column<int>(type: "int", nullable: false),
                    calories_estimated = table.Column<decimal>(type: "decimal(10,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingSessions", x => x.session_id);
                    table.ForeignKey(
                        name: "FK_TrainingSessions_Devices_device_id",
                        column: x => x.device_id,
                        principalTable: "Devices",
                        principalColumn: "device_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MealRecipes",
                columns: table => new
                {
                    meal_id = table.Column<int>(type: "int", nullable: false),
                    recipe_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealRecipes", x => new { x.meal_id, x.recipe_id });
                    table.ForeignKey(
                        name: "FK_MealRecipes_Meals_meal_id",
                        column: x => x.meal_id,
                        principalTable: "Meals",
                        principalColumn: "meal_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MealRecipes_Recipes_recipe_id",
                        column: x => x.recipe_id,
                        principalTable: "Recipes",
                        principalColumn: "recipe_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Recommendations",
                columns: table => new
                {
                    recommendation_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    meal_instance_id = table.Column<int>(type: "int", nullable: true),
                    recommendation_created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    recommendation_type = table.Column<int>(type: "int", nullable: false),
                    recommendation_payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    recommendation_status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recommendations", x => x.recommendation_id);
                    table.ForeignKey(
                        name: "FK_Recommendations_Meals_meal_instance_id",
                        column: x => x.meal_instance_id,
                        principalTable: "Meals",
                        principalColumn: "meal_id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyDietPlans_template_diet_plan_id",
                table: "DailyDietPlans",
                column: "template_diet_plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_DailyDietPlans_user_id",
                table: "DailyDietPlans",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_user_id",
                table: "Devices",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_MealRecipes_recipe_id",
                table: "MealRecipes",
                column: "recipe_id");

            migrationBuilder.CreateIndex(
                name: "IX_Meals_daily_diet_plan_id",
                table: "Meals",
                column: "daily_diet_plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeProducts_product_id",
                table: "RecipeProducts",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_meal_instance_id",
                table: "Recommendations",
                column: "meal_instance_id");

            migrationBuilder.CreateIndex(
                name: "IX_SleepRecords_device_id",
                table: "SleepRecords",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_TelemetrySamples_device_id",
                table: "TelemetrySamples",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingSessions_device_id",
                table: "TrainingSessions",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_user_id",
                table: "UserProfiles",
                column: "user_id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MealRecipes");

            migrationBuilder.DropTable(
                name: "RecipeProducts");

            migrationBuilder.DropTable(
                name: "Recommendations");

            migrationBuilder.DropTable(
                name: "SleepRecords");

            migrationBuilder.DropTable(
                name: "TelemetrySamples");

            migrationBuilder.DropTable(
                name: "TrainingSessions");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Recipes");

            migrationBuilder.DropTable(
                name: "Meals");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "DailyDietPlans");

            migrationBuilder.DropTable(
                name: "TemplateDietPlans");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
