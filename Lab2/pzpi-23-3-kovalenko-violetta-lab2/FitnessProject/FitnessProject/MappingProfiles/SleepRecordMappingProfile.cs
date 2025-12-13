using AutoMapper;
using FitnessProject.BLL.DTO.SleepRecord;
using FitnessProject.Entities;

namespace FitnessProject.MappingProfiles;

public class SleepRecordMappingProfile : Profile
{
    public SleepRecordMappingProfile()
    {
        CreateMap<SleepRecord, SleepRecordResponseDto>();
        CreateMap<SleepRecordCreateDto, SleepRecord>()
            .ForMember(dest => dest.SleepId, opt => opt.Ignore())
            .ForMember(dest => dest.Device, opt => opt.Ignore());
        CreateMap<SleepRecordUpdateDto, SleepRecord>()
            .ForMember(dest => dest.Device, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}

