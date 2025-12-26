using FitnessProject.BLL.DTO.User;

namespace FitnessProject.BLL.Services.Interfaces;

public interface IUserService : IService<Entities.User, UserCreateDto, UserUpdateDto, UserResponseDto>
{
    Task<UserDetailsDto?> GetUserDetailsByIdAsync(int id);
}

