namespace FitnessProject.BLL.DTO.Services;

public class TrainingAggregateDto
{
    public int TrainingCount { get; set; }
    public int TrainingDurationMinutes { get; set; }
    public decimal? TrainingIntensityAvg { get; set; }
    public decimal TrainingCalories { get; set; }
}

