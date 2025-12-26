using Microsoft.EntityFrameworkCore;
using FitnessProject.Data;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.DAL.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetProductDetailsByIdAsync(int id)
    {
        return await _dbSet
            .Include(p => p.RecipeProducts)
                .ThenInclude(rp => rp.Recipe)
            .FirstOrDefaultAsync(p => p.ProductId == id);
    }
}

