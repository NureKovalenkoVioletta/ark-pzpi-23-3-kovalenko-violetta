namespace FitnessProject.BLL.DTO.Services;

public class SleepQualityAnalysis
{
    public decimal? AverageSleepHours { get; set; }
    public decimal? AverageDeepSleepPercent { get; set; }
    public decimal? AverageQuality { get; set; }
    public bool IsSleepDeprived { get; set; }
}

