using AutoMapper;
using FitnessProject.BLL.DTO.MealRecipe;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.BLL.Services;

public class MealRecipeService : IMealRecipeService
{
    private readonly IMealRecipeRepository _repository;
    private readonly IMapper _mapper;

    public MealRecipeService(IMealRecipeRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<MealRecipeResponseDto?> GetByIdAsync(int id)
    {
        var results = await _repository.FindAsync(mr => mr.MealId == id);
        var entity = results.FirstOrDefault();
        return entity == null ? null : _mapper.Map<MealRecipeResponseDto>(entity);
    }

    public async Task<IEnumerable<MealRecipeResponseDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<MealRecipeResponseDto>>(entities);
    }

    public async Task<MealRecipeResponseDto> CreateAsync(MealRecipeCreateDto createDto)
    {
        var entity = _mapper.Map<MealRecipe>(createDto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<MealRecipeResponseDto>(created);
    }

    public async Task<MealRecipeResponseDto> UpdateAsync(MealRecipeUpdateDto updateDto)
    {
        var results = await _repository.FindAsync(mr => mr.MealId == updateDto.MealId && mr.RecipeId == updateDto.RecipeId);
        var entity = results.FirstOrDefault();
        if (entity == null)
            throw new KeyNotFoundException($"MealRecipe with MealId {updateDto.MealId} and RecipeId {updateDto.RecipeId} not found.");

        _mapper.Map(updateDto, entity);
        await _repository.UpdateAsync(entity);
        return _mapper.Map<MealRecipeResponseDto>(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var results = await _repository.FindAsync(mr => mr.MealId == id);
        var entity = results.FirstOrDefault();
        if (entity == null) return false;
        return false;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        var results = await _repository.FindAsync(mr => mr.MealId == id);
        return results.Any();
    }
}

