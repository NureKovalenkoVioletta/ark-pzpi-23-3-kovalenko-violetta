using FitnessProject.Entities;

namespace FitnessProject.DAL.Interfaces;

public interface IRecipeRepository : IRepository<Recipe>
{
    Task<Recipe?> GetRecipeDetailsByIdAsync(int id);
}

