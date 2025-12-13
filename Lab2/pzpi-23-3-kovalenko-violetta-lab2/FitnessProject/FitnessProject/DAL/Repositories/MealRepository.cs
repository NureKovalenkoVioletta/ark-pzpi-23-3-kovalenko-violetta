using FitnessProject.Data;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.DAL.Repositories;

public class MealRepository : Repository<Meal>, IMealRepository
{
    public MealRepository(ApplicationDbContext context) : base(context)
    {
    }
}

