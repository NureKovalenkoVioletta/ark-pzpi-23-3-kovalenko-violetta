namespace FitnessProject.BLL.DTO.Services;

public class WeeklyStatisticsDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<DailyStatisticsDto> Days { get; set; } = new();

    // Aggregated totals/averages for the week
    public decimal TotalSteps { get; set; }
    public decimal? HeartRateAvg { get; set; }
    public decimal? HeartRateMin { get; set; }
    public decimal? HeartRateMax { get; set; }

    public int TotalSleepMinutes { get; set; }
    public int DeepSleepMinutes { get; set; }
    public int LightSleepMinutes { get; set; }
    public int AwakeMinutes { get; set; }
    public decimal? SleepQualityAvg { get; set; }

    public int TrainingCount { get; set; }
    public int TrainingDurationMinutes { get; set; }
    public decimal? TrainingIntensityAvg { get; set; }
    public decimal TrainingCalories { get; set; }

    // Trends (first 3 days vs last 3 days)
    public decimal? StepsTrendPercent { get; set; }
    public decimal? HeartRateAvgTrendPercent { get; set; }
    public decimal? SleepMinutesTrendPercent { get; set; }
    public decimal? TrainingDurationTrendPercent { get; set; }
    public decimal? TrainingCaloriesTrendPercent { get; set; }
}

