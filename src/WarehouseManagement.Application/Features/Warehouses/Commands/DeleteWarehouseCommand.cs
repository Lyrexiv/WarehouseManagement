using MediatR;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.Warehouses.Commands;

public record DeleteWarehouseCommand(Guid Id) : IRequest<bool>;

public class DeleteWarehouseCommandHandler
    : IRequestHandler<DeleteWarehouseCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public DeleteWarehouseCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(
        DeleteWarehouseCommand request,
        CancellationToken cancellationToken)
    {
        var warehouse = await _uow.Warehouses.GetByIdAsync(
            request.Id,
            cancellationToken)
            ?? throw new NotFoundException("Warehouse", request.Id);

        warehouse.IsDeleted = true;
        warehouse.UpdatedAt = DateTime.UtcNow;

        await _uow.Warehouses.UpdateAsync(
            warehouse,
            cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);

        return true;
    }
}