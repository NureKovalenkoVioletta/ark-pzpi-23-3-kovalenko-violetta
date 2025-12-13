using FitnessProject.Enums;

namespace FitnessProject.BLL.DTO.TelemetrySample;

public class TelemetrySampleUpdateDto
{
    public int SampleId { get; set; }
    public int? DeviceId { get; set; }
    public DateTime? Timestamp { get; set; }
    public TelemetryType? TelemetryType { get; set; }
    public decimal? TelemetryValue { get; set; }
}

