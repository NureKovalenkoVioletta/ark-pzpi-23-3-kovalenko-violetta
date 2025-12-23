using AutoMapper;
using FitnessProject.BLL.DTO.DailyDietPlan;
using FitnessProject.Entities;

namespace FitnessProject.MappingProfiles;

public class DailyDietPlanMappingProfile : Profile
{
    public DailyDietPlanMappingProfile()
    {
        CreateMap<DailyDietPlan, DailyDietPlanResponseDto>();
        CreateMap<DailyDietPlan, DailyDietPlanDetailsDto>();
        CreateMap<DailyDietPlanCreateDto, DailyDietPlan>()
            .ForMember(dest => dest.DailyDietPlanId, opt => opt.Ignore())
            .ForMember(dest => dest.DailyPlanCreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.TemplateDietPlan, opt => opt.Ignore())
            .ForMember(dest => dest.Meals, opt => opt.Ignore());
        CreateMap<DailyDietPlanUpdateDto, DailyDietPlan>()
            .ForMember(dest => dest.DailyPlanCreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.TemplateDietPlan, opt => opt.Ignore())
            .ForMember(dest => dest.Meals, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}

