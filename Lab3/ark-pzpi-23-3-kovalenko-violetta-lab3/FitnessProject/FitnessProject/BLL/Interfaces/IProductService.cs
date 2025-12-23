using FitnessProject.BLL.DTO.Product;

namespace FitnessProject.BLL.Services.Interfaces;

public interface IProductService : IService<Entities.Product, ProductCreateDto, ProductUpdateDto, ProductResponseDto>
{
    Task<ProductDetailsDto?> GetProductDetailsByIdAsync(int id);
}

