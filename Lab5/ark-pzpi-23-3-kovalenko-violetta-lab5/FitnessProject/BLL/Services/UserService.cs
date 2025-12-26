using AutoMapper;
using FitnessProject.BLL.DTO.User;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.BLL.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IMapper _mapper;

    public UserService(IUserRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<UserResponseDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : _mapper.Map<UserResponseDto>(entity);
    }

    public async Task<UserDetailsDto?> GetUserDetailsByIdAsync(int id)
    {
        var entity = await _repository.GetUserDetailsByIdAsync(id);
        return entity == null ? null : _mapper.Map<UserDetailsDto>(entity);
    }

    public async Task<IEnumerable<UserResponseDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<UserResponseDto>>(entities);
    }

    public async Task<UserResponseDto> CreateAsync(UserCreateDto createDto)
    {
        var entity = _mapper.Map<User>(createDto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<UserResponseDto>(created);
    }

    public async Task<UserResponseDto> UpdateAsync(UserUpdateDto updateDto)
    {
        var entity = await _repository.GetByIdAsync(updateDto.UserId);
        if (entity == null)
            throw new KeyNotFoundException($"User with ID {updateDto.UserId} not found.");

        _mapper.Map(updateDto, entity);
        await _repository.UpdateAsync(entity);
        return _mapper.Map<UserResponseDto>(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (!await _repository.ExistsAsync(id))
            return false;

        await _repository.DeleteAsync(id);
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _repository.ExistsAsync(id);
    }
}

