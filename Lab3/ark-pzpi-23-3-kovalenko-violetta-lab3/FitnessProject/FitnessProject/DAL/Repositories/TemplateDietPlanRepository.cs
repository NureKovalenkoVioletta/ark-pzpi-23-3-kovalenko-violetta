using Microsoft.EntityFrameworkCore;
using FitnessProject.Data;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.DAL.Repositories;

public class TemplateDietPlanRepository : Repository<TemplateDietPlan>, ITemplateDietPlanRepository
{
    public TemplateDietPlanRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<TemplateDietPlan?> GetTemplateDietPlanDetailsByIdAsync(int id)
    {
        return await _dbSet
            .Include(t => t.DailyDietPlans)
            .FirstOrDefaultAsync(t => t.TemplateDietPlanId == id);
    }
}

