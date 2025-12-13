using FitnessProject.Data;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.DAL.Repositories;

public class RecipeProductRepository : Repository<RecipeProduct>, IRecipeProductRepository
{
    public RecipeProductRepository(ApplicationDbContext context) : base(context)
    {
    }
}

