namespace FitnessProject.BLL.DTO.Services;

public class TelemetryAggregateDto
{
    public decimal Steps { get; set; }
    public decimal? HeartRateAvg { get; set; }
    public decimal? HeartRateMin { get; set; }
    public decimal? HeartRateMax { get; set; }
    public int HeartRateSamples { get; set; }
}

