using AutoMapper;
using FitnessProject.BLL.DTO.Recommendation;
using FitnessProject.Entities;

namespace FitnessProject.MappingProfiles;

public class RecommendationMappingProfile : Profile
{
    public RecommendationMappingProfile()
    {
        CreateMap<Recommendation, RecommendationResponseDto>();
        CreateMap<Recommendation, RecommendationDetailsDto>();
        CreateMap<RecommendationCreateDto, Recommendation>()
            .ForMember(dest => dest.RecommendationId, opt => opt.Ignore())
            .ForMember(dest => dest.RecommendationCreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Meal, opt => opt.Ignore());
        CreateMap<RecommendationUpdateDto, Recommendation>()
            .ForMember(dest => dest.RecommendationCreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Meal, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}

