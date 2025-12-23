using FitnessProject.BLL.DTO.TelemetrySample;

namespace FitnessProject.BLL.Services.Interfaces;

public interface ITelemetrySampleService : IService<Entities.TelemetrySample, TelemetrySampleCreateDto, TelemetrySampleUpdateDto, TelemetrySampleResponseDto>
{
    Task<TelemetrySampleDetailsDto?> GetTelemetrySampleDetailsByIdAsync(int id);
}

