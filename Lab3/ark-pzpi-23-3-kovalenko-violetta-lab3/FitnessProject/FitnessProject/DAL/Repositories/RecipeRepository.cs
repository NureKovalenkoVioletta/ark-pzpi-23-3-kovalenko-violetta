using Microsoft.EntityFrameworkCore;
using FitnessProject.Data;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.DAL.Repositories;

public class RecipeRepository : Repository<Recipe>, IRecipeRepository
{
    public RecipeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Recipe?> GetRecipeDetailsByIdAsync(int id)
    {
        return await _dbSet
            .Include(r => r.MealRecipes)
            .Include(r => r.RecipeProducts)
                .ThenInclude(rp => rp.Product)
            .FirstOrDefaultAsync(r => r.RecipeId == id);
    }
}

