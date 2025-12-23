using AutoMapper;
using FitnessProject.BLL.DTO.TrainingSession;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.BLL.Services;

public class TrainingSessionService : ITrainingSessionService
{
    private readonly ITrainingSessionRepository _repository;
    private readonly IMapper _mapper;

    public TrainingSessionService(ITrainingSessionRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<TrainingSessionResponseDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : _mapper.Map<TrainingSessionResponseDto>(entity);
    }

    public async Task<TrainingSessionDetailsDto?> GetTrainingSessionDetailsByIdAsync(int id)
    {
        var entity = await _repository.GetTrainingSessionDetailsByIdAsync(id);
        return entity == null ? null : _mapper.Map<TrainingSessionDetailsDto>(entity);
    }

    public async Task<IEnumerable<TrainingSessionResponseDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<TrainingSessionResponseDto>>(entities);
    }

    public async Task<TrainingSessionResponseDto> CreateAsync(TrainingSessionCreateDto createDto)
    {
        var entity = _mapper.Map<TrainingSession>(createDto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<TrainingSessionResponseDto>(created);
    }

    public async Task<TrainingSessionResponseDto> UpdateAsync(TrainingSessionUpdateDto updateDto)
    {
        var entity = await _repository.GetByIdAsync(updateDto.SessionId);
        if (entity == null)
            throw new KeyNotFoundException($"TrainingSession with ID {updateDto.SessionId} not found.");

        _mapper.Map(updateDto, entity);
        await _repository.UpdateAsync(entity);
        return _mapper.Map<TrainingSessionResponseDto>(entity);
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

