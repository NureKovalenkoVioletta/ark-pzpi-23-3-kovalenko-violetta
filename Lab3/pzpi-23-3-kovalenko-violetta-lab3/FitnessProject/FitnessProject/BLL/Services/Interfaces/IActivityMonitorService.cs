using FitnessProject.BLL.DTO.Services;

namespace FitnessProject.BLL.Services.Interfaces;

public interface IActivityMonitorService
{
    Task<ActivityChangeResult> CheckActivityChangesAsync(int userId, DateTime date);
}

