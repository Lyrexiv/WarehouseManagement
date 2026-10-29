using MediatR;
using WarehouseManagement.Application.Features.Categories.DTOs;

namespace WarehouseManagement.Application.Features.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(
    CreateCategoryDto Dto
) : IRequest<CategoryDto>;