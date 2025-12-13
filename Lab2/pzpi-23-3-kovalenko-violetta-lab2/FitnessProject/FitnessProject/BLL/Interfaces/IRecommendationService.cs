using FitnessProject.BLL.DTO.Recommendation;

namespace FitnessProject.BLL.Services.Interfaces;

public interface IRecommendationService : IService<Entities.Recommendation, RecommendationCreateDto, RecommendationUpdateDto, RecommendationResponseDto>
{
}

