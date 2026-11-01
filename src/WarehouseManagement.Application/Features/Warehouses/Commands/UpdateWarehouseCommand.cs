using AutoMapper;
using MediatR;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;
using WarehouseManagement.Application.Features.Warehouses.DTOs;

namespace WarehouseManagement.Application.Features.Warehouses.Commands;

public record UpdateWarehouseCommand(
    Guid Id,
    UpdateWarehouseDto Dto
) : IRequest<WarehouseDto>;

public class UpdateWarehouseCommandHandler
    : IRequestHandler<UpdateWarehouseCommand, WarehouseDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UpdateWarehouseCommandHandler(
        IUnitOfWork uow,
        IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<WarehouseDto> Handle(
        UpdateWarehouseCommand request,
        CancellationToken cancellationToken)
    {
        var warehouse = await _uow.Warehouses.GetByIdAsync(
            request.Id,
            cancellationToken)
            ?? throw new NotFoundException("Warehouse", request.Id);

        if (await _uow.Warehouses.CodeExistsAsync(
            request.Dto.Code,
            request.Id,
            cancellationToken))
        {
            throw new DuplicateCodeException(
                "Warehouse",
                request.Dto.Code);
        }

        _mapper.Map(request.Dto, warehouse);

        warehouse.UpdatedAt = DateTime.UtcNow;

        await _uow.Warehouses.UpdateAsync(
            warehouse,
            cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WarehouseDto>(warehouse);
    }
}