using FitnessProject.BLL.DTO.Meal;

namespace FitnessProject.BLL.DTO.DailyDietPlan;

public class DailyDietPlanMealsDto
{
    public int DailyDietPlanId { get; set; }
    public string DailyDietPlanName { get; set; } = string.Empty;
    public DateTime DailyPlanCreatedAt { get; set; }
    public ICollection<MealDetailsDto> Meals { get; set; } = new List<MealDetailsDto>();
}


