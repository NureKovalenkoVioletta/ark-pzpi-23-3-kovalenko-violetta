using AutoMapper;
using FitnessProject.BLL.DTO.RecipeProduct;
using FitnessProject.Entities;

namespace FitnessProject.MappingProfiles;

public class RecipeProductMappingProfile : Profile
{
    public RecipeProductMappingProfile()
    {
        CreateMap<RecipeProduct, RecipeProductResponseDto>();
        CreateMap<RecipeProduct, RecipeProductDetailsDto>();
        CreateMap<RecipeProductCreateDto, RecipeProduct>()
            .ForMember(dest => dest.Recipe, opt => opt.Ignore())
            .ForMember(dest => dest.Product, opt => opt.Ignore());
        CreateMap<RecipeProductUpdateDto, RecipeProduct>()
            .ForMember(dest => dest.Recipe, opt => opt.Ignore())
            .ForMember(dest => dest.Product, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}

