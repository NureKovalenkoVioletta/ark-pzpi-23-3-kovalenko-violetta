using FitnessProject.Entities;

namespace FitnessProject.DAL.Interfaces;

public interface IRecommendationRepository : IRepository<Recommendation>
{
    Task<Recommendation?> GetRecommendationDetailsByIdAsync(int id);
}

