using FitnessProject.Data;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.DAL.Repositories;

public class DailyDietPlanRepository : Repository<DailyDietPlan>, IDailyDietPlanRepository
{
    public DailyDietPlanRepository(ApplicationDbContext context) : base(context)
    {
    }
}

