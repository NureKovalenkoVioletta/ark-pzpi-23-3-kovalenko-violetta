using Microsoft.EntityFrameworkCore;
using FitnessProject.Data;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.DAL.Repositories;

public class MealRecipeRepository : Repository<MealRecipe>, IMealRecipeRepository
{
    public MealRecipeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<MealRecipe?> GetMealRecipeDetailsByIdAsync(int mealId, int recipeId)
    {
        return await _dbSet
            .Include(mr => mr.Meal)
            .Include(mr => mr.Recipe)
            .FirstOrDefaultAsync(mr => mr.MealId == mealId && mr.RecipeId == recipeId);
    }
}

