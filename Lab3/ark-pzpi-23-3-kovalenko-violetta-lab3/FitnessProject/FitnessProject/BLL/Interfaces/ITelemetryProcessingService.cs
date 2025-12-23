using FitnessProject.BLL.DTO.Telemetry;

namespace FitnessProject.BLL.Services.Interfaces;

public interface ITelemetryProcessingService
{
    Task ProcessTelemetryAsync(TelemetryReceiveDto dto);
    Task ProcessBatchAsync(TelemetryReceiveBatchDto batchDto);
}

