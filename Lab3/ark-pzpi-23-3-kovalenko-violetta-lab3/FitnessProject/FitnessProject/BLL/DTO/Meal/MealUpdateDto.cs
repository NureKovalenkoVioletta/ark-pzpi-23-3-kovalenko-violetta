using FitnessProject.Enums;

namespace FitnessProject.BLL.DTO.Meal;

public class MealUpdateDto
{
    public int MealId { get; set; }
    public int? DailyDietPlanId { get; set; }
    public MealTime? MealTime { get; set; }
    public int? MealOrder { get; set; }
    public decimal? MealTargetCalories { get; set; }
    public decimal? MealTargetFat { get; set; }
    public decimal? MealTargetCarbs { get; set; }
    public decimal? MealTargetProtein { get; set; }
}

