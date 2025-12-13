using AutoMapper;
using FitnessProject.BLL.DTO.SleepRecord;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.BLL.Services;

public class SleepRecordService : ISleepRecordService
{
    private readonly ISleepRecordRepository _repository;
    private readonly IMapper _mapper;

    public SleepRecordService(ISleepRecordRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<SleepRecordResponseDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : _mapper.Map<SleepRecordResponseDto>(entity);
    }

    public async Task<IEnumerable<SleepRecordResponseDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<SleepRecordResponseDto>>(entities);
    }

    public async Task<SleepRecordResponseDto> CreateAsync(SleepRecordCreateDto createDto)
    {
        var entity = _mapper.Map<SleepRecord>(createDto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<SleepRecordResponseDto>(created);
    }

    public async Task<SleepRecordResponseDto> UpdateAsync(SleepRecordUpdateDto updateDto)
    {
        var entity = await _repository.GetByIdAsync(updateDto.SleepId);
        if (entity == null)
            throw new KeyNotFoundException($"SleepRecord with ID {updateDto.SleepId} not found.");

        _mapper.Map(updateDto, entity);
        await _repository.UpdateAsync(entity);
        return _mapper.Map<SleepRecordResponseDto>(entity);
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

