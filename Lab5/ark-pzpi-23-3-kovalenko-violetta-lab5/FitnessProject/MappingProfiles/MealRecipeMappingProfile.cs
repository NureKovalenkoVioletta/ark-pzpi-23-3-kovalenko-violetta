using AutoMapper;
using FitnessProject.BLL.DTO.MealRecipe;
using FitnessProject.Entities;

namespace FitnessProject.MappingProfiles;

public class MealRecipeMappingProfile : Profile
{
    public MealRecipeMappingProfile()
    {
        CreateMap<MealRecipe, MealRecipeResponseDto>();
        CreateMap<MealRecipe, MealRecipeDetailsDto>();
        CreateMap<MealRecipeCreateDto, MealRecipe>()
            .ForMember(dest => dest.Meal, opt => opt.Ignore())
            .ForMember(dest => dest.Recipe, opt => opt.Ignore());
        CreateMap<MealRecipeUpdateDto, MealRecipe>()
            .ForMember(dest => dest.Meal, opt => opt.Ignore())
            .ForMember(dest => dest.Recipe, opt => opt.Ignore());
    }
}

