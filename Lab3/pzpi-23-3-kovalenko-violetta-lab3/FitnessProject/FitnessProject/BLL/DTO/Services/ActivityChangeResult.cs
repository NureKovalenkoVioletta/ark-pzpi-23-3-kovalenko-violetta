namespace FitnessProject.BLL.DTO.Services;

public class ActivityChangeResult
{
    public WeeklyActivityAverage WeeklyAverage { get; set; } = new();

    public decimal StepsToday { get; set; }
    public decimal? HeartRateToday { get; set; }
    public decimal? TrainingIntensityToday { get; set; }

    public decimal? StepsChangePercent { get; set; }
    public decimal? HeartRateChangePercent { get; set; }
    public decimal? TrainingIntensityChangePercent { get; set; }

    public bool StepsSpike { get; set; }
    public bool TrainingIntensityChange { get; set; }
    public bool HeartRateAnomaly { get; set; }
}

