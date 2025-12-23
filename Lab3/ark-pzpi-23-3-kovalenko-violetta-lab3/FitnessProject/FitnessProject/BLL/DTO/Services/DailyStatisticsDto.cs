namespace FitnessProject.BLL.DTO.Services;

public class DailyStatisticsDto
{
    // Telemetry (per day)
    public decimal Steps { get; set; }
    public decimal? HeartRateAvg { get; set; }
    public decimal? HeartRateMin { get; set; }
    public decimal? HeartRateMax { get; set; }
    public int HeartRateSamples { get; set; }

    // Sleep (per day)
    public int TotalSleepMinutes { get; set; }
    public int DeepSleepMinutes { get; set; }
    public int LightSleepMinutes { get; set; }
    public int AwakeMinutes { get; set; }
    public decimal? SleepQualityAvg { get; set; }

    // Trainings (per day)
    public int TrainingCount { get; set; }
    public int TrainingDurationMinutes { get; set; }
    public decimal? TrainingIntensityAvg { get; set; }
    public decimal TrainingCalories { get; set; }
}

