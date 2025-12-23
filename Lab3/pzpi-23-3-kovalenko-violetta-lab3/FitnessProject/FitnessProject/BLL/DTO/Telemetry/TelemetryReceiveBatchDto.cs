using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FitnessProject.BLL.DTO.Telemetry;

public class TelemetryReceiveBatchDto
{
    [Required]
    [MinLength(1, ErrorMessage = "Batch must contain at least one item")]
    [MaxLength(1000, ErrorMessage = "Batch cannot contain more than 1000 items")]
    [JsonPropertyName("items")]
    public List<TelemetryReceiveDto> Items { get; set; } = new();
}

