using Microsoft.EntityFrameworkCore;
using FitnessProject.Data;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.DAL.Repositories;

public class TelemetrySampleRepository : Repository<TelemetrySample>, ITelemetrySampleRepository
{
    public TelemetrySampleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<TelemetrySample?> GetTelemetrySampleDetailsByIdAsync(int id)
    {
        return await _dbSet
            .Include(t => t.Device)
            .FirstOrDefaultAsync(t => t.SampleId == id);
    }

    public async Task<TelemetrySample> AddWithoutSaveAsync(TelemetrySample entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public async Task UpdateWithoutSaveAsync(TelemetrySample entity)
    {
        _dbSet.Update(entity);
        await Task.CompletedTask;
    }
}

