using AutoMapper;
using FitnessProject.BLL.DTO.TrainingSession;
using FitnessProject.Entities;

namespace FitnessProject.MappingProfiles;

public class TrainingSessionMappingProfile : Profile
{
    public TrainingSessionMappingProfile()
    {
        CreateMap<TrainingSession, TrainingSessionResponseDto>();
        CreateMap<TrainingSession, TrainingSessionDetailsDto>();
        CreateMap<TrainingSessionCreateDto, TrainingSession>()
            .ForMember(dest => dest.SessionId, opt => opt.Ignore())
            .ForMember(dest => dest.Device, opt => opt.Ignore());
        CreateMap<TrainingSessionUpdateDto, TrainingSession>()
            .ForMember(dest => dest.Device, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}

