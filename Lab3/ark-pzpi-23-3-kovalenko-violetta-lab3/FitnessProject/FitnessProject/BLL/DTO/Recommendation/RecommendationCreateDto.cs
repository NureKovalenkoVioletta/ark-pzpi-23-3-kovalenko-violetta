using FitnessProject.Enums;

namespace FitnessProject.BLL.DTO.Recommendation;

public class RecommendationCreateDto
{
    public int? MealInstanceId { get; set; }
    public RecommendationType RecommendationType { get; set; }
    public string? RecommendationPayload { get; set; }
    public RecommendationStatus RecommendationStatus { get; set; }
}

