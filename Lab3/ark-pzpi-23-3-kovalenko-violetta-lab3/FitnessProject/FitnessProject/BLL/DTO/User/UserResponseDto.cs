using FitnessProject.Enums;

namespace FitnessProject.BLL.DTO.User;

public class UserResponseDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Locale { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}

