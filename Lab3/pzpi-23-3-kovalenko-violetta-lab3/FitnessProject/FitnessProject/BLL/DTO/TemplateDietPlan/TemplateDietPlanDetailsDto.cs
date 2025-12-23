using FitnessProject.BLL.DTO.DailyDietPlan;
using FitnessProject.Enums;

namespace FitnessProject.BLL.DTO.TemplateDietPlan;

public class TemplateDietPlanDetailsDto
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
    
    public ICollection<DailyDietPlanResponseDto> DailyDietPlans { get; set; } = new List<DailyDietPlanResponseDto>();
}

