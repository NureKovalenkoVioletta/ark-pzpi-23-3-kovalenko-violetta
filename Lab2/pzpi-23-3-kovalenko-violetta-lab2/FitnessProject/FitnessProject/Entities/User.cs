using FitnessProject.Enums;

namespace FitnessProject.Entities;

public class User
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Locale { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    public UserProfile? UserProfile { get; set; }
    public ICollection<DailyDietPlan> DailyDietPlans { get; set; } = new List<DailyDietPlan>();
    public ICollection<Device> Devices { get; set; } = new List<Device>();
}

