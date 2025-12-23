using FitnessProject.Entities;

namespace FitnessProject.DAL.Interfaces;

public interface IMealRepository : IRepository<Meal>
{
    Task<Meal?> GetMealDetailsByIdAsync(int id);
}

