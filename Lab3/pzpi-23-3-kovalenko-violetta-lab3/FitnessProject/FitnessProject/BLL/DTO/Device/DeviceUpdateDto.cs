using FitnessProject.Enums;

namespace FitnessProject.BLL.DTO.Device;

public class DeviceUpdateDto
{
    public int DeviceId { get; set; }
    public int? UserId { get; set; }
    public DeviceType? DeviceType { get; set; }
    public ConnectionType? ConnectionType { get; set; }
    public DateTime? LastSeen { get; set; }
    public string? Serial { get; set; }
}

