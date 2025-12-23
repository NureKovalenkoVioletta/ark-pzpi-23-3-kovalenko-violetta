using FitnessProject.Entities;

namespace FitnessProject.DAL.Interfaces;

public interface IDeviceRepository : IRepository<Device>
{
    Task<Device?> GetDeviceDetailsByIdAsync(int id);
    Task UpdateWithoutSaveAsync(Device entity);
}

