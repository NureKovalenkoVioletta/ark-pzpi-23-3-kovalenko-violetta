using AutoMapper;
using FitnessProject.BLL.DTO.UserProfile;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.BLL.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _repository;
    private readonly IMapper _mapper;

    public UserProfileService(IUserProfileRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<UserProfileResponseDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : _mapper.Map<UserProfileResponseDto>(entity);
    }

    public async Task<UserProfileDetailsDto?> GetUserProfileDetailsByIdAsync(int id)
    {
        var entity = await _repository.GetUserProfileDetailsByIdAsync(id);
        return entity == null ? null : _mapper.Map<UserProfileDetailsDto>(entity);
    }

    public async Task<IEnumerable<UserProfileResponseDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<UserProfileResponseDto>>(entities);
    }

    public async Task<UserProfileResponseDto> CreateAsync(UserProfileCreateDto createDto)
    {
        var entity = _mapper.Map<UserProfile>(createDto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<UserProfileResponseDto>(created);
    }

    public async Task<UserProfileResponseDto> UpdateAsync(UserProfileUpdateDto updateDto)
    {
        var entity = await _repository.GetByIdAsync(updateDto.ProfileId);
        if (entity == null)
            throw new KeyNotFoundException($"UserProfile with ID {updateDto.ProfileId} not found.");

        _mapper.Map(updateDto, entity);
        await _repository.UpdateAsync(entity);
        return _mapper.Map<UserProfileResponseDto>(entity);
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

