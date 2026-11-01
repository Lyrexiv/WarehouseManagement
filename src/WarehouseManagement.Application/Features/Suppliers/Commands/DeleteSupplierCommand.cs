using MediatR;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.Suppliers.Commands;

public record DeleteSupplierCommand(Guid Id) : IRequest<bool>;

public class DeleteSupplierCommandHandler
    : IRequestHandler<DeleteSupplierCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public DeleteSupplierCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(
        DeleteSupplierCommand request,
        CancellationToken cancellationToken)
    {
        var supplier = await _uow.Suppliers.GetByIdAsync(
            request.Id,
            cancellationToken)
            ?? throw new NotFoundException("Supplier", request.Id);

        supplier.IsDeleted = true;
        supplier.UpdatedAt = DateTime.UtcNow;

        await _uow.Suppliers.UpdateAsync(
            supplier,
            cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);

        return true;
    }
}