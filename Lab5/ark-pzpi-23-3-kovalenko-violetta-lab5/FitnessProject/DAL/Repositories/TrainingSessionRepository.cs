using Microsoft.EntityFrameworkCore;
using FitnessProject.Data;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.DAL.Repositories;

public class TrainingSessionRepository : Repository<TrainingSession>, ITrainingSessionRepository
{
    public TrainingSessionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<TrainingSession?> GetTrainingSessionDetailsByIdAsync(int id)
    {
        return await _dbSet
            .Include(t => t.Device)
            .FirstOrDefaultAsync(t => t.SessionId == id);
    }
}

