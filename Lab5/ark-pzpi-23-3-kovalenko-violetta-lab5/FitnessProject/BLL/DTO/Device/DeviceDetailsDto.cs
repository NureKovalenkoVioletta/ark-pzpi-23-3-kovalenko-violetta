using FitnessProject.BLL.DTO.User;
using FitnessProject.BLL.DTO.TelemetrySample;
using FitnessProject.BLL.DTO.SleepRecord;
using FitnessProject.BLL.DTO.TrainingSession;
using FitnessProject.Enums;

namespace FitnessProject.BLL.DTO.Device;

public class DeviceDetailsDto
{
    public int DeviceId { get; set; }
    public int UserId { get; set; }
    public DeviceType DeviceType { get; set; }
    public ConnectionType ConnectionType { get; set; }
    public DateTime? LastSeen { get; set; }
    public string? Serial { get; set; }
    
    public UserResponseDto User { get; set; } = null!;
    public ICollection<TelemetrySampleResponseDto> TelemetrySamples { get; set; } = new List<TelemetrySampleResponseDto>();
    public ICollection<SleepRecordResponseDto> SleepRecords { get; set; } = new List<SleepRecordResponseDto>();
    public ICollection<TrainingSessionResponseDto> TrainingSessions { get; set; } = new List<TrainingSessionResponseDto>();
}

