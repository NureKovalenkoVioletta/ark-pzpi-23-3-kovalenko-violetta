using FitnessProject.Data;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.DAL.Repositories;

public class TrainingSessionRepository : Repository<TrainingSession>, ITrainingSessionRepository
{
    public TrainingSessionRepository(ApplicationDbContext context) : base(context)
    {
    }
}

