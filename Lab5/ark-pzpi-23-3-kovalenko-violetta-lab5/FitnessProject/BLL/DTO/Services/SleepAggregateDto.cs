namespace FitnessProject.BLL.DTO.Services;

public class SleepAggregateDto
{
    public int TotalSleepMinutes { get; set; }
    public int DeepSleepMinutes { get; set; }
    public int LightSleepMinutes { get; set; }
    public int AwakeMinutes { get; set; }
    public decimal? SleepQualityAvg { get; set; }
}

