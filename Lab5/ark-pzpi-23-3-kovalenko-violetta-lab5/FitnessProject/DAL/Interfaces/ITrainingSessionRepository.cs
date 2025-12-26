using FitnessProject.Entities;

namespace FitnessProject.DAL.Interfaces;

public interface ITrainingSessionRepository : IRepository<TrainingSession>
{
    Task<TrainingSession?> GetTrainingSessionDetailsByIdAsync(int id);
}

