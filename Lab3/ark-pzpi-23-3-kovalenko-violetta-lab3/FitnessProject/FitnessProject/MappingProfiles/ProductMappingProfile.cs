using AutoMapper;
using FitnessProject.BLL.DTO.Product;
using FitnessProject.Entities;

namespace FitnessProject.MappingProfiles;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductResponseDto>();
        CreateMap<Product, ProductDetailsDto>();
        CreateMap<ProductCreateDto, Product>()
            .ForMember(dest => dest.ProductId, opt => opt.Ignore())
            .ForMember(dest => dest.RecipeProducts, opt => opt.Ignore());
        CreateMap<ProductUpdateDto, Product>()
            .ForMember(dest => dest.RecipeProducts, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}

