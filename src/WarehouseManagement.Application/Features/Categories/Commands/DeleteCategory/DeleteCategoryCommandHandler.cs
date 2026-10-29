using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler
    : IRequestHandler<
        DeleteCategoryCommand,
        bool>
{
    private readonly IUnitOfWork _uow;

    public DeleteCategoryCommandHandler(
        IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(
        DeleteCategoryCommand req,
        CancellationToken ct)
    {
        var c =
            await _uow.Categories
                .GetByIdAsync(req.Id, ct)
            ?? throw new NotFoundException(
                "Category",
                req.Id);

        c.IsDeleted = true;
        c.UpdatedAt = DateTime.UtcNow;

        await _uow.Categories.UpdateAsync(c, ct);

        await _uow.SaveChangesAsync(ct);

        return true;
    }
}