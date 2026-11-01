using AutoMapper;
using MediatR;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;
using WarehouseManagement.Application.Features.Suppliers.DTOs;

namespace WarehouseManagement.Application.Features.Suppliers.Queries;

public record GetSupplierByIdQuery(Guid Id)
    : IRequest<SupplierDto>;

public class GetSupplierByIdQueryHandler
    : IRequestHandler<GetSupplierByIdQuery, SupplierDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetSupplierByIdQueryHandler(
        IUnitOfWork uow,
        IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<SupplierDto> Handle(
        GetSupplierByIdQuery request,
        CancellationToken cancellationToken)
    {
        var supplier = await _uow.Suppliers.GetByIdAsync(
            request.Id,
            cancellationToken)
            ?? throw new NotFoundException("Supplier", request.Id);

        if (supplier.IsDeleted)
        {
            throw new NotFoundException("Supplier", request.Id);
        }

        return _mapper.Map<SupplierDto>(supplier);
    }
}