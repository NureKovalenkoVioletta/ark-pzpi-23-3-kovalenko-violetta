using Microsoft.EntityFrameworkCore;
using FitnessProject.Data;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.DAL.Repositories;

public class DeviceRepository : Repository<Device>, IDeviceRepository
{
    public DeviceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Device?> GetDeviceDetailsByIdAsync(int id)
    {
        return await _dbSet
            .Include(d => d.User)
            .Include(d => d.TelemetrySamples)
            .Include(d => d.SleepRecords)
            .Include(d => d.TrainingSessions)
            .FirstOrDefaultAsync(d => d.DeviceId == id);
    }

    public async Task UpdateWithoutSaveAsync(Device entity)
    {
        _dbSet.Update(entity);
        await Task.CompletedTask;
    }
}

