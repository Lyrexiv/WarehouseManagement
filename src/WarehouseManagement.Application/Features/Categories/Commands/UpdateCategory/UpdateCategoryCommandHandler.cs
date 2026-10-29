using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.Categories.Commands.UpdateCategory;
using WarehouseManagement.Application.Features.Categories.DTOs;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.Categories.Commands;

public class UpdateCategoryCommandHandler
    : IRequestHandler<
        UpdateCategoryCommand,
        CategoryDto>
{
    private readonly IUnitOfWork _uow;

    public UpdateCategoryCommandHandler(
        IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<CategoryDto> Handle(
        UpdateCategoryCommand req,
        CancellationToken ct)
    {
        var c =
            await _uow.Categories
                .GetByIdAsync(req.Id, ct)
            ?? throw new NotFoundException(
                "Category",
                req.Id);

        if (await _uow.Categories
            .NameExistsAsync(
                req.Dto.Name,
                req.Id,
                ct))
        {
            throw new BusinessException(
                $"Category name '{req.Dto.Name}' already taken");
        }

        c.Name = req.Dto.Name;
        c.Description = req.Dto.Description;
        c.Code = req.Dto.Code;
        c.IsActive = req.Dto.IsActive;
        c.UpdatedAt = DateTime.UtcNow;

        await _uow.Categories.UpdateAsync(c, ct);

        await _uow.SaveChangesAsync(ct);

        return new CategoryDto(
            c.Id,
            c.Name,
            c.Description,
            c.Code,
            c.IsActive,
            c.CreatedAt);
    }
}