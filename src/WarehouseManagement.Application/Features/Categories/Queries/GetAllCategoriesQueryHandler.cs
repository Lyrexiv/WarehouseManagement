using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.Categories.DTOs;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.Categories.Queries;

public class GetAllCategoriesQueryHandler
    : IRequestHandler<
        GetAllCategoriesQuery,
        IEnumerable<CategoryDto>>
{
    private readonly IUnitOfWork _uow;

    public GetAllCategoriesQueryHandler(
        IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<CategoryDto>> Handle(
        GetAllCategoriesQuery req,
        CancellationToken ct)
    {
        var all =
            await _uow.Categories.GetAllAsync(ct);

        return all
            .Where(c => !c.IsDeleted)
            .Select(c =>
                new CategoryDto(
                    c.Id,
                    c.Name,
                    c.Description,
                    c.Code,
                    c.IsActive,
                    c.CreatedAt));
    }
}