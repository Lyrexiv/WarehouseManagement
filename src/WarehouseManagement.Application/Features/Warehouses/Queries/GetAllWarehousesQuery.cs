using AutoMapper;
using MediatR;
using WarehouseManagement.Application.Common.Models;
using WarehouseManagement.Application.Features.Warehouses.DTOs;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.Warehouses.Queries;

public record GetAllWarehousesQuery(PaginationParams Params)
    : IRequest<PagedList<WarehouseDto>>;

public class GetAllWarehousesQueryHandler
    : IRequestHandler<GetAllWarehousesQuery, PagedList<WarehouseDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetAllWarehousesQueryHandler(
        IUnitOfWork uow,
        IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PagedList<WarehouseDto>> Handle(
        GetAllWarehousesQuery request,
        CancellationToken cancellationToken)
    {
        var all = await _uow.Warehouses.GetAllAsync(
            cancellationToken);

        var query = all
            .Where(w => !w.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Params.SearchTerm))
        {
            var searchTerm = request.Params.SearchTerm.Trim();

            query = query.Where(w =>
                w.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                w.Code.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        var totalCount = query.Count();

        var warehouses = query
            .OrderBy(w => w.Name)
            .Skip((request.Params.PageNumber - 1) * request.Params.PageSize)
            .Take(request.Params.PageSize)
            .ToList();

        var warehouseDtos = _mapper.Map<List<WarehouseDto>>(warehouses);

        return new PagedList<WarehouseDto>(
            warehouseDtos,
            totalCount,
            request.Params.PageNumber,
            request.Params.PageSize);
    }
}