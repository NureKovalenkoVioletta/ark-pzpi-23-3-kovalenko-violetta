using FitnessProject.BLL.DTO.UserProfile;
using FitnessProject.BLL.DTO.DailyDietPlan;
using FitnessProject.BLL.DTO.Device;
using FitnessProject.Enums;

namespace FitnessProject.BLL.DTO.User;

public class UserDetailsDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Locale { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    
    public UserProfileResponseDto? UserProfile { get; set; }
    public ICollection<DailyDietPlanResponseDto> DailyDietPlans { get; set; } = new List<DailyDietPlanResponseDto>();
    public ICollection<DeviceResponseDto> Devices { get; set; } = new List<DeviceResponseDto>();
}

