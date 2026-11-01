using AutoMapper;
using MediatR;
using WarehouseManagement.Application.Features.Warehouses.DTOs;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.Warehouses.Queries;

public record GetWarehouseByIdQuery(Guid Id)
    : IRequest<WarehouseDto>;

public class GetWarehouseByIdQueryHandler
    : IRequestHandler<GetWarehouseByIdQuery, WarehouseDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetWarehouseByIdQueryHandler(
        IUnitOfWork uow,
        IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<WarehouseDto> Handle(
        GetWarehouseByIdQuery request,
        CancellationToken cancellationToken)
    {
        var warehouse = await _uow.Warehouses.GetByIdAsync(
            request.Id,
            cancellationToken)
            ?? throw new NotFoundException("Warehouse", request.Id);

        if (warehouse.IsDeleted)
        {
            throw new NotFoundException("Warehouse", request.Id);
        }

        return _mapper.Map<WarehouseDto>(warehouse);
    }
}