using AutoMapper;
using FitnessProject.BLL.DTO.Product;
using FitnessProject.BLL.Services.Interfaces;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.BLL.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;

    public ProductService(IProductRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ProductResponseDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity == null ? null : _mapper.Map<ProductResponseDto>(entity);
    }

    public async Task<IEnumerable<ProductResponseDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<ProductResponseDto>>(entities);
    }

    public async Task<ProductResponseDto> CreateAsync(ProductCreateDto createDto)
    {
        var entity = _mapper.Map<Product>(createDto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<ProductResponseDto>(created);
    }

    public async Task<ProductResponseDto> UpdateAsync(ProductUpdateDto updateDto)
    {
        var entity = await _repository.GetByIdAsync(updateDto.ProductId);
        if (entity == null)
            throw new KeyNotFoundException($"Product with ID {updateDto.ProductId} not found.");

        _mapper.Map(updateDto, entity);
        await _repository.UpdateAsync(entity);
        return _mapper.Map<ProductResponseDto>(entity);
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

