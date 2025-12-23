using Microsoft.EntityFrameworkCore;
using FitnessProject.Data;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.DAL.Repositories;

public class DailyDietPlanRepository : Repository<DailyDietPlan>, IDailyDietPlanRepository
{
    public DailyDietPlanRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<DailyDietPlan?> GetDailyDietPlanDetailsByIdAsync(int id)
    {
        return await _dbSet
            .Include(d => d.User)
            .Include(d => d.TemplateDietPlan)
            .Include(d => d.Meals)
            .FirstOrDefaultAsync(d => d.DailyDietPlanId == id);
    }
}

