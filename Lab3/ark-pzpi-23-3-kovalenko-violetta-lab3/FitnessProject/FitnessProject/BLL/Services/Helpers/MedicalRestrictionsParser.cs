using FitnessProject.BLL.DTO.MedicalRestrictions;
using FitnessProject.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FitnessProject.BLL.Services.Helpers;

public static class MedicalRestrictionsParser
{
    public static UserMedicalRestrictionsDto ParseMedicalConditions(string? medicalConditions)
    {
        if (string.IsNullOrWhiteSpace(medicalConditions))
        {
            return new UserMedicalRestrictionsDto();
        }

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            options.Converters.Add(new JsonStringEnumConverter());

            var parsed = JsonSerializer.Deserialize<UserMedicalRestrictionsDto>(medicalConditions, options);
            return parsed ?? new UserMedicalRestrictionsDto();
        }
        catch
        {
            var result = new UserMedicalRestrictionsDto();
            
            var parts = medicalConditions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var part in parts)
            {
                if (Enum.TryParse<DietaryRestrictionType>(part, true, out var restriction))
                {
                    result.DietaryRestriction = restriction;
                }
                else
                {
                    result.MedicalConditions.Add(part);
                }
            }
            
            return result;
        }
    }

    public static string SerializeMedicalConditions(UserMedicalRestrictionsDto restrictions)
    {
        return JsonSerializer.Serialize(restrictions);
    }
}

