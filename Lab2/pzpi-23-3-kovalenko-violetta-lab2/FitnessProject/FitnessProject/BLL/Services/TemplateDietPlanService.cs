using AutoMapper;
using FitnessProject.BLL.DTO.TemplateDietPlan;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.BLL.Services;

public class TemplateDietPlanService : ITemplateDietPlanService
{
    private readonly ITemplateDietPlanRepository _repository;
    private readonly IMapper _mapper;

    public TemplateDietPlanService(ITemplateDietPlanRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<TemplateDietPlanResponseDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : _mapper.Map<TemplateDietPlanResponseDto>(entity);
    }

    public async Task<IEnumerable<TemplateDietPlanResponseDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<TemplateDietPlanResponseDto>>(entities);
    }

    public async Task<TemplateDietPlanResponseDto> CreateAsync(TemplateDietPlanCreateDto createDto)
    {
        var entity = _mapper.Map<TemplateDietPlan>(createDto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<TemplateDietPlanResponseDto>(created);
    }

    public async Task<TemplateDietPlanResponseDto> UpdateAsync(TemplateDietPlanUpdateDto updateDto)
    {
        var entity = await _repository.GetByIdAsync(updateDto.TemplateDietPlanId);
        if (entity == null)
            throw new KeyNotFoundException($"TemplateDietPlan with ID {updateDto.TemplateDietPlanId} not found.");

        _mapper.Map(updateDto, entity);
        await _repository.UpdateAsync(entity);
        return _mapper.Map<TemplateDietPlanResponseDto>(entity);
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

