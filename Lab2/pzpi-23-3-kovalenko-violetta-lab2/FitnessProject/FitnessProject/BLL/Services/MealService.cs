using AutoMapper;
using FitnessProject.BLL.DTO.Meal;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.BLL.Services;

public class MealService : IMealService
{
    private readonly IMealRepository _repository;
    private readonly IMapper _mapper;

    public MealService(IMealRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<MealResponseDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : _mapper.Map<MealResponseDto>(entity);
    }

    public async Task<IEnumerable<MealResponseDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<MealResponseDto>>(entities);
    }

    public async Task<MealResponseDto> CreateAsync(MealCreateDto createDto)
    {
        var entity = _mapper.Map<Meal>(createDto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<MealResponseDto>(created);
    }

    public async Task<MealResponseDto> UpdateAsync(MealUpdateDto updateDto)
    {
        var entity = await _repository.GetByIdAsync(updateDto.MealId);
        if (entity == null)
            throw new KeyNotFoundException($"Meal with ID {updateDto.MealId} not found.");

        _mapper.Map(updateDto, entity);
        await _repository.UpdateAsync(entity);
        return _mapper.Map<MealResponseDto>(entity);
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

