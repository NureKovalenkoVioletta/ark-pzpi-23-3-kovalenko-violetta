using FitnessProject.Enums;

namespace FitnessProject.Entities;

public class DailyDietPlan
{
    public int DailyDietPlanId { get; set; }
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
    public DateTime DailyPlanCreatedAt { get; set; }
    public bool IsCorrected { get; set; }

    public User User { get; set; } = null!;
    public TemplateDietPlan? TemplateDietPlan { get; set; }
    public ICollection<Meal> Meals { get; set; } = new List<Meal>();
}

