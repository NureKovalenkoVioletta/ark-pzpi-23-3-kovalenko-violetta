using FitnessProject.BLL.DTO.UserProfile;

namespace FitnessProject.BLL.Services.Interfaces;

public interface IUserProfileService : IService<Entities.UserProfile, UserProfileCreateDto, UserProfileUpdateDto, UserProfileResponseDto>
{
    Task<UserProfileDetailsDto?> GetUserProfileDetailsByIdAsync(int id);
}

