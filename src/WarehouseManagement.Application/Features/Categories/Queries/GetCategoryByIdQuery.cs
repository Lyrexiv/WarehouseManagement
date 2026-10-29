using MediatR;
using WarehouseManagement.Application.Features.Categories.DTOs;

namespace WarehouseManagement.Application.Features.Categories.Queries;

public record GetCategoryByIdQuery(
    Guid Id
) : IRequest<CategoryDto>;