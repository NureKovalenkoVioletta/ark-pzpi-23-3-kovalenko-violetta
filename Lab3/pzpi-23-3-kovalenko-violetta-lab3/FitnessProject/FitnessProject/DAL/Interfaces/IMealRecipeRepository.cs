using FitnessProject.Entities;

namespace FitnessProject.DAL.Interfaces;

public interface IMealRecipeRepository : IRepository<MealRecipe>
{
    Task<MealRecipe?> GetMealRecipeDetailsByIdAsync(int mealId, int recipeId);
}

