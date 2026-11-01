using AutoMapper;
using WarehouseManagement.Application.Features.Products.DTOs;
using WarehouseManagement.Domain.Entities;

namespace WarehouseManagement.Application.Common.Mappings;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        // Entity -> DTO
        CreateMap<Product, ProductDto>()
            .ForCtorParam(nameof(ProductDto.CategoryName), opt =>
                opt.MapFrom(src => src.Category.Name));

        // Create DTO -> Entity
        CreateMap<CreateProductDto, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // Update DTO -> Entity
        CreateMap<UpdateProductDto, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
    }
}