using FitnessProject.Data;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.DAL.Repositories;

public class SleepRecordRepository : Repository<SleepRecord>, ISleepRecordRepository
{
    public SleepRecordRepository(ApplicationDbContext context) : base(context)
    {
    }
}

