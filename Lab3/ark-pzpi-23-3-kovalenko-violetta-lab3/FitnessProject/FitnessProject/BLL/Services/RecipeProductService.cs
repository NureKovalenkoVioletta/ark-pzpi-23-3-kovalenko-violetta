using AutoMapper;
using FitnessProject.BLL.DTO.RecipeProduct;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.BLL.Services;

public class RecipeProductService : IRecipeProductService
{
    private readonly IRecipeProductRepository _repository;
    private readonly IMapper _mapper;

    public RecipeProductService(IRecipeProductRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<RecipeProductResponseDto?> GetByIdAsync(int id)
    {
        var results = await _repository.FindAsync(rp => rp.RecipeId == id);
        var entity = results.FirstOrDefault();
        return entity == null ? null : _mapper.Map<RecipeProductResponseDto>(entity);
    }

    public async Task<RecipeProductDetailsDto?> GetRecipeProductDetailsByIdAsync(int recipeId, int productId)
    {
        var entity = await _repository.GetRecipeProductDetailsByIdAsync(recipeId, productId);
        return entity == null ? null : _mapper.Map<RecipeProductDetailsDto>(entity);
    }

    public async Task<IEnumerable<RecipeProductResponseDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<RecipeProductResponseDto>>(entities);
    }

    public async Task<RecipeProductResponseDto> CreateAsync(RecipeProductCreateDto createDto)
    {
        var entity = _mapper.Map<RecipeProduct>(createDto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<RecipeProductResponseDto>(created);
    }

    public async Task<RecipeProductResponseDto> UpdateAsync(RecipeProductUpdateDto updateDto)
    {
        var results = await _repository.FindAsync(rp => rp.RecipeId == updateDto.RecipeId && rp.ProductId == updateDto.ProductId);
        var entity = results.FirstOrDefault();
        if (entity == null)
            throw new KeyNotFoundException($"RecipeProduct not found.");

        _mapper.Map(updateDto, entity);
        await _repository.UpdateAsync(entity);
        return _mapper.Map<RecipeProductResponseDto>(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var results = await _repository.FindAsync(rp => rp.RecipeId == id);
        var entity = results.FirstOrDefault();
        if (entity == null) return false;
        return false;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        var results = await _repository.FindAsync(rp => rp.RecipeId == id);
        return results.Any();
    }
}

