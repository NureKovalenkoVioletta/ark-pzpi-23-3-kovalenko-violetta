using FitnessProject.Enums;

namespace FitnessProject.BLL.DTO.User;

public class UserUpdateDto
{
    public int UserId { get; set; }
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public string? Locale { get; set; }
    public UserRole? Role { get; set; }
}

