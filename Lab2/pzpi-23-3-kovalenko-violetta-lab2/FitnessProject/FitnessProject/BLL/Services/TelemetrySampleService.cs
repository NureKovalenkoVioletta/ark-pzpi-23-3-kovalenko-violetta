using AutoMapper;
using FitnessProject.BLL.DTO.TelemetrySample;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.BLL.Services;

public class TelemetrySampleService : ITelemetrySampleService
{
    private readonly ITelemetrySampleRepository _repository;
    private readonly IMapper _mapper;

    public TelemetrySampleService(ITelemetrySampleRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<TelemetrySampleResponseDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : _mapper.Map<TelemetrySampleResponseDto>(entity);
    }

    public async Task<IEnumerable<TelemetrySampleResponseDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<TelemetrySampleResponseDto>>(entities);
    }

    public async Task<TelemetrySampleResponseDto> CreateAsync(TelemetrySampleCreateDto createDto)
    {
        var entity = _mapper.Map<TelemetrySample>(createDto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<TelemetrySampleResponseDto>(created);
    }

    public async Task<TelemetrySampleResponseDto> UpdateAsync(TelemetrySampleUpdateDto updateDto)
    {
        var entity = await _repository.GetByIdAsync(updateDto.SampleId);
        if (entity == null)
            throw new KeyNotFoundException($"TelemetrySample with ID {updateDto.SampleId} not found.");

        _mapper.Map(updateDto, entity);
        await _repository.UpdateAsync(entity);
        return _mapper.Map<TelemetrySampleResponseDto>(entity);
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

