using FitnessProject.Entities;

namespace FitnessProject.DAL.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetProductDetailsByIdAsync(int id);
}

