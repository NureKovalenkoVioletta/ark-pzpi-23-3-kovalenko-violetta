using FitnessProject.Enums;

namespace FitnessProject.BLL.DTO.User;

public class UserCreateDto
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Locale { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}

