using FitnessProject.Enums;

namespace FitnessProject.Entities;

public class Recommendation
{
    public int RecommendationId { get; set; }
    public int? MealInstanceId { get; set; }
    public DateTime RecommendationCreatedAt { get; set; }
    public RecommendationType RecommendationType { get; set; }
    public string? RecommendationPayload { get; set; }
    public RecommendationStatus RecommendationStatus { get; set; }

    public Meal? Meal { get; set; }
}

