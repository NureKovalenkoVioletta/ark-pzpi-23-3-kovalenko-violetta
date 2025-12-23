using FitnessProject.Enums;

namespace FitnessProject.Entities;

public class Device
{
    public int DeviceId { get; set; }
    public int UserId { get; set; }
    public DeviceType DeviceType { get; set; }
    public ConnectionType ConnectionType { get; set; }
    public DateTime? LastSeen { get; set; }
    public string? Serial { get; set; }

    public User User { get; set; } = null!;
    public ICollection<TelemetrySample> TelemetrySamples { get; set; } = new List<TelemetrySample>();
    public ICollection<SleepRecord> SleepRecords { get; set; } = new List<SleepRecord>();
    public ICollection<TrainingSession> TrainingSessions { get; set; } = new List<TrainingSession>();
}

