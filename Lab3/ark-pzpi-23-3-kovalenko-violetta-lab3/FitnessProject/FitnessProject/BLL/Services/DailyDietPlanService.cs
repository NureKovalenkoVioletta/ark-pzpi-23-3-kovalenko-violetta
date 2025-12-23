using AutoMapper;
using FitnessProject.BLL.DTO.DailyDietPlan;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.BLL.Services;

public class DailyDietPlanService : IDailyDietPlanService
{
    private readonly IDailyDietPlanRepository _repository;
    private readonly IMapper _mapper;

    public DailyDietPlanService(IDailyDietPlanRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<DailyDietPlanResponseDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : _mapper.Map<DailyDietPlanResponseDto>(entity);
    }

    public async Task<DailyDietPlanDetailsDto?> GetDailyDietPlanDetailsByIdAsync(int id)
    {
        var entity = await _repository.GetDailyDietPlanDetailsByIdAsync(id);
        return entity == null ? null : _mapper.Map<DailyDietPlanDetailsDto>(entity);
    }

    public async Task<IEnumerable<DailyDietPlanResponseDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<DailyDietPlanResponseDto>>(entities);
    }

    public async Task<DailyDietPlanResponseDto> CreateAsync(DailyDietPlanCreateDto createDto)
    {
        var entity = _mapper.Map<DailyDietPlan>(createDto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<DailyDietPlanResponseDto>(created);
    }

    public async Task<DailyDietPlanResponseDto> UpdateAsync(DailyDietPlanUpdateDto updateDto)
    {
        var entity = await _repository.GetByIdAsync(updateDto.DailyDietPlanId);
        if (entity == null)
            throw new KeyNotFoundException($"DailyDietPlan with ID {updateDto.DailyDietPlanId} not found.");

        _mapper.Map(updateDto, entity);
        await _repository.UpdateAsync(entity);
        return _mapper.Map<DailyDietPlanResponseDto>(entity);
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

