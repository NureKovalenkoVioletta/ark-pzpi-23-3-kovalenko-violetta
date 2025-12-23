using FitnessProject.Entities;

namespace FitnessProject.DAL.Interfaces;

public interface ISleepRecordRepository : IRepository<SleepRecord>
{
    Task<SleepRecord?> GetSleepRecordDetailsByIdAsync(int id);
    Task<SleepRecord> AddWithoutSaveAsync(SleepRecord entity);
    Task UpdateWithoutSaveAsync(SleepRecord entity);
}

