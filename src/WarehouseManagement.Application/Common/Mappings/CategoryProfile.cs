using AutoMapper;
using WarehouseManagement.Application.Features.Categories.DTOs;
using WarehouseManagement.Domain.Entities;

namespace WarehouseManagement.Application.Common.Mappings;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        // Entity -> DTO
        CreateMap<Category, CategoryDto>();

        // Create DTO -> Entity
        CreateMap<CreateCategoryDto, Category>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

        // Update DTO -> Entity
        CreateMap<UpdateCategoryDto, Category>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
    }
}