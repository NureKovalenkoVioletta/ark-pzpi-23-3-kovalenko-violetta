namespace FitnessProject.BLL.DTO.SleepRecord;

public class SleepRecordCreateDto
{
    public int DeviceId { get; set; }
    public DateTime Date { get; set; }
    public int TotalSleepMinutes { get; set; }
    public int DeepSleepMinutes { get; set; }
    public int LightSleepMinutes { get; set; }
    public int AwakeMinutes { get; set; }
    public decimal? SleepQuality { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}

