using AutoMapper;
using FitnessProject.BLL.DTO.Recipe;
using FitnessProject.Entities;

namespace FitnessProject.MappingProfiles;

public class RecipeMappingProfile : Profile
{
    public RecipeMappingProfile()
    {
        CreateMap<Recipe, RecipeResponseDto>();
        CreateMap<RecipeCreateDto, Recipe>()
            .ForMember(dest => dest.RecipeId, opt => opt.Ignore())
            .ForMember(dest => dest.MealRecipes, opt => opt.Ignore())
            .ForMember(dest => dest.RecipeProducts, opt => opt.Ignore());
        CreateMap<RecipeUpdateDto, Recipe>()
            .ForMember(dest => dest.MealRecipes, opt => opt.Ignore())
            .ForMember(dest => dest.RecipeProducts, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}

