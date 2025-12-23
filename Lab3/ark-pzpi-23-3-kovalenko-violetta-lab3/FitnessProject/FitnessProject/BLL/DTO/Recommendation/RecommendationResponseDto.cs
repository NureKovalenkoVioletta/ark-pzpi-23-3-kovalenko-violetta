using FitnessProject.Enums;

namespace FitnessProject.BLL.DTO.Recommendation;

public class RecommendationResponseDto
{
    public int RecommendationId { get; set; }
    public int? MealInstanceId { get; set; }
    public DateTime RecommendationCreatedAt { get; set; }
    public RecommendationType RecommendationType { get; set; }
    public string? RecommendationPayload { get; set; }
    public RecommendationStatus RecommendationStatus { get; set; }
}

