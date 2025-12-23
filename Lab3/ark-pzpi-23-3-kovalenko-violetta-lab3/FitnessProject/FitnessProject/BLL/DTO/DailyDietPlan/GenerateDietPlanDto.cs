using System.ComponentModel.DataAnnotations;

namespace FitnessProject.BLL.DTO.DailyDietPlan;

public class GenerateDietPlanDto
{
    [Required]
    public int UserId { get; set; }

    public DateTime? Date { get; set; }

    public int? TemplateDietPlanId { get; set; }
}


