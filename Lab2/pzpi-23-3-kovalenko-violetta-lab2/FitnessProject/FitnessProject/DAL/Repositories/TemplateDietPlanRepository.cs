using FitnessProject.Data;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.DAL.Repositories;

public class TemplateDietPlanRepository : Repository<TemplateDietPlan>, ITemplateDietPlanRepository
{
    public TemplateDietPlanRepository(ApplicationDbContext context) : base(context)
    {
    }
}

