using AutoMapper;
using FitnessProject.BLL.DTO.TelemetrySample;
using FitnessProject.Entities;

namespace FitnessProject.MappingProfiles;

public class TelemetrySampleMappingProfile : Profile
{
    public TelemetrySampleMappingProfile()
    {
        CreateMap<TelemetrySample, TelemetrySampleResponseDto>();
        CreateMap<TelemetrySampleCreateDto, TelemetrySample>()
            .ForMember(dest => dest.SampleId, opt => opt.Ignore())
            .ForMember(dest => dest.Device, opt => opt.Ignore());
        CreateMap<TelemetrySampleUpdateDto, TelemetrySample>()
            .ForMember(dest => dest.Device, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}

