namespace FitnessProject.BLL.DTO.Services;

public class WeekComparisonDto
{
    public WeeklyStatisticsDto CurrentWeek { get; set; } = new();
    public WeeklyStatisticsDto PreviousWeek { get; set; } = new();

    public decimal? StepsChangePercent { get; set; }
    public decimal? HeartRateAvgChangePercent { get; set; }
    public decimal? TotalSleepChangePercent { get; set; }
    public decimal? TrainingDurationChangePercent { get; set; }
    public decimal? TrainingCaloriesChangePercent { get; set; }
}

