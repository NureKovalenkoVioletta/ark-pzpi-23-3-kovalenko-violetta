using FitnessProject.Enums;

namespace FitnessProject.BLL.DTO.UserProfile;

public class UserProfileCreateDto
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Sex Sex { get; set; }
    public decimal HeightCm { get; set; }
    public decimal CurrentWeightKg { get; set; }
    public ActivityLevel ActivityLevel { get; set; }
    public string? MedicalConditions { get; set; }
    public PreferredUnits PreferredUnits { get; set; }
    public DateTime? BirthDate { get; set; }
}

