using AutoMapper;
using MediatR;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;
using WarehouseManagement.Application.Features.Suppliers.DTOs;

namespace WarehouseManagement.Application.Features.Suppliers.Commands;

// Command
public record UpdateSupplierCommand(
    Guid Id,
    UpdateSupplierDto Dto
) : IRequest<SupplierDto>;

// Handler
public class UpdateSupplierCommandHandler
    : IRequestHandler<UpdateSupplierCommand, SupplierDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UpdateSupplierCommandHandler(
        IUnitOfWork uow,
        IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<SupplierDto> Handle(
        UpdateSupplierCommand request,
        CancellationToken cancellationToken)
    {
        var supplier = await _uow.Suppliers.GetByIdAsync(
            request.Id,
            cancellationToken)
            ?? throw new NotFoundException("Supplier", request.Id);

        if (await _uow.Suppliers.CodeExistsAsync(
            request.Dto.Code,
            request.Id,
            cancellationToken))
        {
            throw new DuplicateCodeException(
                "Supplier",
                request.Dto.Code);
        }

        _mapper.Map(request.Dto, supplier);

        supplier.UpdatedAt = DateTime.UtcNow;

        await _uow.Suppliers.UpdateAsync(
            supplier,
            cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);

        return _mapper.Map<SupplierDto>(supplier);
    }
}