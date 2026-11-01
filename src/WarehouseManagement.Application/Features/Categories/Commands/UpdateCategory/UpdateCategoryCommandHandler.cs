using AutoMapper;
using MediatR;
using WarehouseManagement.Application.Features.Categories.DTOs;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler
    : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public UpdateCategoryCommandHandler(
        IUnitOfWork uow,
        IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<CategoryDto> Handle(
        UpdateCategoryCommand req,
        CancellationToken ct)
    {
        var category = await _uow.Categories.GetByIdAsync(
            req.Id, ct)
            ?? throw new NotFoundException("Category", req.Id);

        // Check duplicate name
        if (await _uow.Categories.NameExistsAsync(
            req.Dto.Name,
            req.Id,
            ct))
        {
            throw new BusinessException(
                $"Category name '{req.Dto.Name}' already taken");
        }

        // Mapping DTO -> existing Entity
        _mapper.Map(req.Dto, category);

        category.UpdatedAt = DateTime.UtcNow;

        await _uow.Categories.UpdateAsync(category, ct);

        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<CategoryDto>(category);
    }
}