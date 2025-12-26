using AutoMapper;
using FitnessProject.BLL.DTO.Device;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.BLL.Services;

public class DeviceService : IDeviceService
{
    private readonly IDeviceRepository _repository;
    private readonly IMapper _mapper;

    public DeviceService(IDeviceRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<DeviceResponseDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : _mapper.Map<DeviceResponseDto>(entity);
    }

    public async Task<DeviceDetailsDto?> GetDeviceDetailsByIdAsync(int id)
    {
        var entity = await _repository.GetDeviceDetailsByIdAsync(id);
        return entity == null ? null : _mapper.Map<DeviceDetailsDto>(entity);
    }

    public async Task<IEnumerable<DeviceResponseDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<DeviceResponseDto>>(entities);
    }

    public async Task<DeviceResponseDto> CreateAsync(DeviceCreateDto createDto)
    {
        var entity = _mapper.Map<Device>(createDto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<DeviceResponseDto>(created);
    }

    public async Task<DeviceResponseDto> UpdateAsync(DeviceUpdateDto updateDto)
    {
        var entity = await _repository.GetByIdAsync(updateDto.DeviceId);
        if (entity == null)
            throw new KeyNotFoundException($"Device with ID {updateDto.DeviceId} not found.");

        _mapper.Map(updateDto, entity);
        await _repository.UpdateAsync(entity);
        return _mapper.Map<DeviceResponseDto>(entity);
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

