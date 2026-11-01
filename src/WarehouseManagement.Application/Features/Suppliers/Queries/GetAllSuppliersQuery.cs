using AutoMapper;
using MediatR;
using WarehouseManagement.Domain.Interfaces;
using WarehouseManagement.Application.Features.Suppliers.DTOs;

namespace WarehouseManagement.Application.Features.Suppliers.Queries;

public record GetAllSuppliersQuery()
    : IRequest<IEnumerable<SupplierDto>>;

public class GetAllSuppliersQueryHandler
    : IRequestHandler<GetAllSuppliersQuery, IEnumerable<SupplierDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetAllSuppliersQueryHandler(
        IUnitOfWork uow,
        IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SupplierDto>> Handle(
        GetAllSuppliersQuery request,
        CancellationToken cancellationToken)
    {
        var suppliers = await _uow.Suppliers.GetAllAsync(
            cancellationToken);

        var activeSuppliers = suppliers
            .Where(s => !s.IsDeleted)
            .ToList();

        return _mapper.Map<IEnumerable<SupplierDto>>(activeSuppliers);
    }
}