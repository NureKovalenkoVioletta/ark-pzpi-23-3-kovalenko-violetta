using FitnessProject.Entities;

namespace FitnessProject.DAL.Interfaces;

public interface IRecipeProductRepository : IRepository<RecipeProduct>
{
    Task<RecipeProduct?> GetRecipeProductDetailsByIdAsync(int recipeId, int productId);
}

