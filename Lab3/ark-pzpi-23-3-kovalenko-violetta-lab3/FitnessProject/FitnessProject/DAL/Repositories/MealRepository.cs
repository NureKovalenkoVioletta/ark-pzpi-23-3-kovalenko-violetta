using Microsoft.EntityFrameworkCore;
using FitnessProject.Data;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.DAL.Repositories;

public class MealRepository : Repository<Meal>, IMealRepository
{
    public MealRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Meal?> GetMealDetailsByIdAsync(int id)
    {
        return await _dbSet
            .Include(m => m.DailyDietPlan)
            .Include(m => m.MealRecipes)
            .Include(m => m.Recommendations)
            .FirstOrDefaultAsync(m => m.MealId == id);
    }
}

