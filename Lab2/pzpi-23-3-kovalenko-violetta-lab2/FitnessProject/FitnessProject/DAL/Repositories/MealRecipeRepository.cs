using FitnessProject.Data;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.DAL.Repositories;

public class MealRecipeRepository : Repository<MealRecipe>, IMealRecipeRepository
{
    public MealRecipeRepository(ApplicationDbContext context) : base(context)
    {
    }
}

