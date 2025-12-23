using Microsoft.EntityFrameworkCore;
using FitnessProject.Data;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.DAL.Repositories;

public class RecommendationRepository : Repository<Recommendation>, IRecommendationRepository
{
    public RecommendationRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Recommendation?> GetRecommendationDetailsByIdAsync(int id)
    {
        return await _dbSet
            .Include(r => r.Meal)
            .FirstOrDefaultAsync(r => r.RecommendationId == id);
    }
}

