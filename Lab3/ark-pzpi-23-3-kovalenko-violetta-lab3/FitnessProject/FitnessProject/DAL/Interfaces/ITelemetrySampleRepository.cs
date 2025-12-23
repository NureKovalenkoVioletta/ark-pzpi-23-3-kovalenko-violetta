using FitnessProject.Entities;

namespace FitnessProject.DAL.Interfaces;

public interface ITelemetrySampleRepository : IRepository<TelemetrySample>
{
    Task<TelemetrySample?> GetTelemetrySampleDetailsByIdAsync(int id);
    Task<TelemetrySample> AddWithoutSaveAsync(TelemetrySample entity);
    Task UpdateWithoutSaveAsync(TelemetrySample entity);
}

