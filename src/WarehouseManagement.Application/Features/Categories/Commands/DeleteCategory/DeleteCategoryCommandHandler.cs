using MediatR;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler
    : IRequestHandler<DeleteCategoryCommand, bool>
{
    private readonly IUnitOfWork _uow;

    public DeleteCategoryCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<bool> Handle(
        DeleteCategoryCommand req,
        CancellationToken ct)
    {
        var category = await _uow.Categories.GetByIdAsync(
            req.Id, ct)
            ?? throw new NotFoundException("Category", req.Id);

        // Soft Delete
        category.IsDeleted = true;
        category.UpdatedAt = DateTime.UtcNow;

        await _uow.Categories.UpdateAsync(category, ct);

        await _uow.SaveChangesAsync(ct);

        return true;
    }
}