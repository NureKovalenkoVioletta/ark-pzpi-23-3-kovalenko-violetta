using FitnessProject.Data;
using FitnessProject.DAL.Interfaces;
using FitnessProject.Entities;

namespace FitnessProject.DAL.Repositories;

public class TelemetrySampleRepository : Repository<TelemetrySample>, ITelemetrySampleRepository
{
    public TelemetrySampleRepository(ApplicationDbContext context) : base(context)
    {
    }
}

