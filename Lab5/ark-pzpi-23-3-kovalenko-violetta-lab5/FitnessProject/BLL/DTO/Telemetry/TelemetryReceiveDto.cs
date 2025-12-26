using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using FitnessProject.Enums;

namespace FitnessProject.BLL.DTO.Telemetry;

public class TelemetryReceiveDto
{
    [Required]
    [JsonPropertyName("deviceId")]
    public int DeviceId { get; set; }

    [Required]
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [Required]
    [JsonPropertyName("telemetryType")]
    public TelemetryType TelemetryType { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Value must be non-negative")]
    [JsonPropertyName("value")]
    public decimal Value { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }
}

