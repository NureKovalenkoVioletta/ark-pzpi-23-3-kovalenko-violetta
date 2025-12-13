using AutoMapper;
using FitnessProject.BLL.DTO.UserProfile;
using FitnessProject.Entities;

namespace FitnessProject.MappingProfiles;

public class UserProfileMappingProfile : Profile
{
    public UserProfileMappingProfile()
    {
        CreateMap<UserProfile, UserProfileResponseDto>();
        CreateMap<UserProfileCreateDto, UserProfile>()
            .ForMember(dest => dest.ProfileId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore());
        CreateMap<UserProfileUpdateDto, UserProfile>()
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}

