using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.Categories.DTOs;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler
    : IRequestHandler<
        CreateCategoryCommand,
        CategoryDto>
{
    private readonly IUnitOfWork _uow;

    public CreateCategoryCommandHandler(
        IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<CategoryDto> Handle(
        CreateCategoryCommand req,
        CancellationToken ct)
    {
        if (await _uow.Categories
            .NameExistsAsync(
                req.Dto.Name,
                null,
                ct))
        {
            throw new BusinessException(
                $"Category '{req.Dto.Name}' already exists");
        }

        var c =
            new Domain.Entities.Category
            {
                Name = req.Dto.Name,
                Description = req.Dto.Description,
                Code = req.Dto.Code
            };

        await _uow.Categories.AddAsync(c, ct);

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