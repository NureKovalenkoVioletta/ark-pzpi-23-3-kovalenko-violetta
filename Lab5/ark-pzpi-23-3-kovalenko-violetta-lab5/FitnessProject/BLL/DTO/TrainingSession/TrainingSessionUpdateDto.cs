using FitnessProject.Enums;

namespace FitnessProject.BLL.DTO.TrainingSession;

public class TrainingSessionUpdateDto
{
    public int SessionId { get; set; }
    public int? DeviceId { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TrainingType? Type { get; set; }
    public Intensity? Intensity { get; set; }
    public int? DurationInMin { get; set; }
    public decimal? CaloriesEstimated { get; set; }
}

