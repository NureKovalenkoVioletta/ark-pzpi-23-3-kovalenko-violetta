using AutoMapper;
using FitnessProject.BLL.DTO.Meal;
using FitnessProject.Entities;

namespace FitnessProject.MappingProfiles;

public class MealMappingProfile : Profile
{
    public MealMappingProfile()
    {
        CreateMap<Meal, MealResponseDto>();
        CreateMap<Meal, MealDetailsDto>();
        CreateMap<MealCreateDto, Meal>()
            .ForMember(dest => dest.MealId, opt => opt.Ignore())
            .ForMember(dest => dest.DailyDietPlan, opt => opt.Ignore())
            .ForMember(dest => dest.MealRecipes, opt => opt.Ignore())
            .ForMember(dest => dest.Recommendations, opt => opt.Ignore());
        CreateMap<MealUpdateDto, Meal>()
            .ForMember(dest => dest.DailyDietPlan, opt => opt.Ignore())
            .ForMember(dest => dest.MealRecipes, opt => opt.Ignore())
            .ForMember(dest => dest.Recommendations, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}

