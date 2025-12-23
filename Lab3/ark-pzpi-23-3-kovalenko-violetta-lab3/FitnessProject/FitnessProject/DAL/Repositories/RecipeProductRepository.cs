using Microsoft.EntityFrameworkCore;
using FitnessProject.Data;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.DAL.Repositories;

public class RecipeProductRepository : Repository<RecipeProduct>, IRecipeProductRepository
{
    public RecipeProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<RecipeProduct?> GetRecipeProductDetailsByIdAsync(int recipeId, int productId)
    {
        return await _dbSet
            .Include(rp => rp.Recipe)
            .Include(rp => rp.Product)
            .FirstOrDefaultAsync(rp => rp.RecipeId == recipeId && rp.ProductId == productId);
    }
}

