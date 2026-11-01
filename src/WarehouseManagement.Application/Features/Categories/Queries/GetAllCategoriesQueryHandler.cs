using AutoMapper;
using MediatR;
using WarehouseManagement.Application.Features.Categories.DTOs;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.Categories.Queries;

public class GetAllCategoriesQueryHandler
    : IRequestHandler<GetAllCategoriesQuery, IEnumerable<CategoryDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetAllCategoriesQueryHandler(
        IUnitOfWork uow,
        IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CategoryDto>> Handle(
        GetAllCategoriesQuery req,
        CancellationToken ct)
    {
        var categories = await _uow.Categories.GetAllAsync(ct);

        var activeCategories = categories
            .Where(c => !c.IsDeleted)
            .ToList();

        return _mapper.Map<List<CategoryDto>>(activeCategories);
    }
}