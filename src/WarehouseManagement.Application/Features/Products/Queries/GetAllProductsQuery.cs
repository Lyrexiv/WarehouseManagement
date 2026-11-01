using AutoMapper;
using MediatR;
using WarehouseManagement.Application.Common.Models;
using WarehouseManagement.Application.Features.Products.DTOs;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.Products.Queries;

public record GetAllProductsQuery(PaginationParams Params)
    : IRequest<PagedList<ProductDto>>;

public class GetAllProductsQueryHandler
    : IRequestHandler<GetAllProductsQuery, PagedList<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllProductsQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedList<ProductDto>> Handle(
        GetAllProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.Products
            .GetWithCategoryAsync(cancellationToken);

        var query = products
            .Where(p => !p.IsDeleted);

        // Search
        if (!string.IsNullOrWhiteSpace(request.Params.SearchTerm))
        {
            var term = request.Params.SearchTerm.Trim();

            query = query.Where(p =>
                p.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                p.SKU.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                p.Barcode.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        // Total records
        var total = query.Count();

        // Pagination
        var productList = query
            .Skip((request.Params.PageNumber - 1) * request.Params.PageSize)
            .Take(request.Params.PageSize)
            .ToList();

        // Mapping Entity -> DTO
        var items = _mapper.Map<List<ProductDto>>(productList);

        return new PagedList<ProductDto>(
            items,
            total,
            request.Params.PageNumber,
            request.Params.PageSize);
    }
}