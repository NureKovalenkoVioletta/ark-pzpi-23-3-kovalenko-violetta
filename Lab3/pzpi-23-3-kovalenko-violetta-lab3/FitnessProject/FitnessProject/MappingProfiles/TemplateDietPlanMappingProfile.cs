using AutoMapper;
using FitnessProject.BLL.DTO.TemplateDietPlan;
using FitnessProject.Entities;

namespace FitnessProject.MappingProfiles;

public class TemplateDietPlanMappingProfile : Profile
{
    public TemplateDietPlanMappingProfile()
    {
        CreateMap<TemplateDietPlan, TemplateDietPlanResponseDto>();
        CreateMap<TemplateDietPlan, TemplateDietPlanDetailsDto>();
        CreateMap<TemplateDietPlanCreateDto, TemplateDietPlan>()
            .ForMember(dest => dest.TemplateDietPlanId, opt => opt.Ignore())
            .ForMember(dest => dest.TemplateCreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.DailyDietPlans, opt => opt.Ignore());
        CreateMap<TemplateDietPlanUpdateDto, TemplateDietPlan>()
            .ForMember(dest => dest.TemplateCreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DailyDietPlans, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}

