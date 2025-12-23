using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitnessProject.Migrations
{
    /// <inheritdoc />
    public partial class AddGoalTypeAndMealRecipePortions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "goal_type",
                table: "UserProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "portions_metadata",
                table: "MealRecipes",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "goal_type",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "portions_metadata",
                table: "MealRecipes");
        }
    }
}
