using FitnessProject.Enums;

namespace FitnessProject.BLL.DTO.UserProfile;

public class UserProfileUpdateDto
{
    public int ProfileId { get; set; }
    public int? UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public Sex? Sex { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? CurrentWeightKg { get; set; }
    public ActivityLevel? ActivityLevel { get; set; }
    public GoalType? GoalType { get; set; }
    public string? MedicalConditions { get; set; }
    public PreferredUnits? PreferredUnits { get; set; }
    public DateTime? BirthDate { get; set; }
}

