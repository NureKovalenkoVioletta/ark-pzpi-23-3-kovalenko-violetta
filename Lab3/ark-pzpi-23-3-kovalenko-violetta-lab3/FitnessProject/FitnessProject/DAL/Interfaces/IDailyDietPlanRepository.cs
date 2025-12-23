using FitnessProject.Entities;

namespace FitnessProject.DAL.Interfaces;

public interface IDailyDietPlanRepository : IRepository<DailyDietPlan>
{
    Task<DailyDietPlan?> GetDailyDietPlanDetailsByIdAsync(int id);
}

