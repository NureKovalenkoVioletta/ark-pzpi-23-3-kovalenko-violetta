using Microsoft.EntityFrameworkCore;
using FitnessProject.Data;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.DAL.Repositories;

public class SleepRecordRepository : Repository<SleepRecord>, ISleepRecordRepository
{
    public SleepRecordRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<SleepRecord?> GetSleepRecordDetailsByIdAsync(int id)
    {
        return await _dbSet
            .Include(s => s.Device)
            .FirstOrDefaultAsync(s => s.SleepId == id);
    }

    public async Task<SleepRecord> AddWithoutSaveAsync(SleepRecord entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public async Task UpdateWithoutSaveAsync(SleepRecord entity)
    {
        _dbSet.Update(entity);
        await Task.CompletedTask;
    }
}

