using FitnessProject.Enums;

namespace FitnessProject.BLL.DTO.MedicalRestrictions;

public class UserMedicalRestrictionsDto
{
    public List<string> Allergens { get; set; } = new();
    public List<string> MedicalConditions { get; set; } = new();
    public DietaryRestrictionType? DietaryRestriction { get; set; }
}

