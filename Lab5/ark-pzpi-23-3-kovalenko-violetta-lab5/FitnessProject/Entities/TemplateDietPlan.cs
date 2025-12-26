using FitnessProject.Enums;

namespace FitnessProject.Entities;

public class TemplateDietPlan
{
    public int TemplateDietPlanId { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string? TemplateDescription { get; set; }
    public decimal TemplateCaloriesMin { get; set; }
    public decimal TemplateCaloriesMax { get; set; }
    public decimal TemplateProteinMin { get; set; }
    public decimal TemplateProteinMax { get; set; }
    public decimal TemplateFatMin { get; set; }
    public decimal TemplateFatMax { get; set; }
    public decimal TemplateCarbsMin { get; set; }
    public decimal TemplateCarbsMax { get; set; }
    public int TemplateNumberOfMeals { get; set; }
    public TemplateStatus TemplateStatus { get; set; }
    public DateTime TemplateCreatedAt { get; set; }

    public ICollection<DailyDietPlan> DailyDietPlans { get; set; } = new List<DailyDietPlan>();
}

