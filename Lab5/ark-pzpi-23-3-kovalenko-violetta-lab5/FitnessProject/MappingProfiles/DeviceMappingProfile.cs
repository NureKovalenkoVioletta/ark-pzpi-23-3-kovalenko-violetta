using AutoMapper;
using FitnessProject.BLL.DTO.Device;
using FitnessProject.Entities;

namespace FitnessProject.MappingProfiles;

public class DeviceMappingProfile : Profile
{
    public DeviceMappingProfile()
    {
        CreateMap<Device, DeviceResponseDto>();
        CreateMap<Device, DeviceDetailsDto>();
        CreateMap<DeviceCreateDto, Device>()
            .ForMember(dest => dest.DeviceId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.TelemetrySamples, opt => opt.Ignore())
            .ForMember(dest => dest.SleepRecords, opt => opt.Ignore())
            .ForMember(dest => dest.TrainingSessions, opt => opt.Ignore());
        CreateMap<DeviceUpdateDto, Device>()
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.TelemetrySamples, opt => opt.Ignore())
            .ForMember(dest => dest.SleepRecords, opt => opt.Ignore())
            .ForMember(dest => dest.TrainingSessions, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}

