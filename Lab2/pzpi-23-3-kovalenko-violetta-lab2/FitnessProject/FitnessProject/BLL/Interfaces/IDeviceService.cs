using FitnessProject.BLL.DTO.Device;

namespace FitnessProject.BLL.Services.Interfaces;

public interface IDeviceService : IService<Entities.Device, DeviceCreateDto, DeviceUpdateDto, DeviceResponseDto>
{
}

