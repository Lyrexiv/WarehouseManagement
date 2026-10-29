using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.Categories.DTOs;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.Categories.Queries;

public class GetCategoryByIdQueryHandler
    : IRequestHandler<
        GetCategoryByIdQuery,
        CategoryDto>
{
    private readonly IUnitOfWork _uow;

    public GetCategoryByIdQueryHandler(
        IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<CategoryDto> Handle(
        GetCategoryByIdQuery req,
        CancellationToken ct)
    {
        var c =
            await _uow.Categories
                .GetByIdAsync(req.Id, ct)
            ?? throw new NotFoundException(
                "Category",
                req.Id);

        return new CategoryDto(
            c.Id,
            c.Name,
            c.Description,
            c.Code,
            c.IsActive,
            c.CreatedAt);
    }
}