using FitnessProject.Enums;

namespace FitnessProject.BLL.DTO.DailyDietPlan;

public class DailyDietPlanCreateDto
{
    public int UserId { get; set; }
    public int? TemplateDietPlanId { get; set; }
    public string DailyDietPlanName { get; set; } = string.Empty;
    public string? DailyPlanDescription { get; set; }
    public decimal DailyPlanCalories { get; set; }
    public decimal DailyPlanFat { get; set; }
    public decimal DailyPlanCarbs { get; set; }
    public decimal DailyPlanProtein { get; set; }
    public int DailyPlanNumberOfMeals { get; set; }
    public DailyPlanStatus DailyPlanStatus { get; set; }
}

