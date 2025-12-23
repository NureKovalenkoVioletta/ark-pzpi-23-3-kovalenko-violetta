using AutoMapper;
using FitnessProject.BLL.DTO.User;
using FitnessProject.Entities;

namespace FitnessProject.MappingProfiles;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserResponseDto>();
        CreateMap<User, UserDetailsDto>();
        CreateMap<UserCreateDto, User>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.UserProfile, opt => opt.Ignore())
            .ForMember(dest => dest.DailyDietPlans, opt => opt.Ignore())
            .ForMember(dest => dest.Devices, opt => opt.Ignore());
        CreateMap<UserUpdateDto, User>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UserProfile, opt => opt.Ignore())
            .ForMember(dest => dest.DailyDietPlans, opt => opt.Ignore())
            .ForMember(dest => dest.Devices, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}

