using MediatR;
using WarehouseManagement.Application.Features.Categories.DTOs;

namespace WarehouseManagement.Application.Features.Categories.Queries;

public record GetAllCategoriesQuery()
    : IRequest<IEnumerable<CategoryDto>>;