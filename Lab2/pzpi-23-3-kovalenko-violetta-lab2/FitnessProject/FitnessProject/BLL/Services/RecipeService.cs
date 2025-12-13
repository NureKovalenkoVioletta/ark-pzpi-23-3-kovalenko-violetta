using AutoMapper;
using FitnessProject.BLL.DTO.Recipe;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.BLL.Services;

public class RecipeService : IRecipeService
{
    private readonly IRecipeRepository _repository;
    private readonly IMapper _mapper;

    public RecipeService(IRecipeRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<RecipeResponseDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : _mapper.Map<RecipeResponseDto>(entity);
    }

    public async Task<IEnumerable<RecipeResponseDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<RecipeResponseDto>>(entities);
    }

    public async Task<RecipeResponseDto> CreateAsync(RecipeCreateDto createDto)
    {
        var entity = _mapper.Map<Recipe>(createDto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<RecipeResponseDto>(created);
    }

    public async Task<RecipeResponseDto> UpdateAsync(RecipeUpdateDto updateDto)
    {
        var entity = await _repository.GetByIdAsync(updateDto.RecipeId);
        if (entity == null)
            throw new KeyNotFoundException($"Recipe with ID {updateDto.RecipeId} not found.");

        _mapper.Map(updateDto, entity);
        await _repository.UpdateAsync(entity);
        return _mapper.Map<RecipeResponseDto>(entity);
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

