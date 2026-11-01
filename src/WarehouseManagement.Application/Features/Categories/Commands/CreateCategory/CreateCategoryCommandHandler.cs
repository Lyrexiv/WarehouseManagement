using AutoMapper;
using MediatR;
using WarehouseManagement.Application.Features.Categories.DTOs;
using WarehouseManagement.Domain.Entities;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler
    : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CreateCategoryCommandHandler(
        IUnitOfWork uow,
        IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<CategoryDto> Handle(
        CreateCategoryCommand req,
        CancellationToken ct)
    {
        // Check duplicate category name
        if (await _uow.Categories.NameExistsAsync(
            req.Dto.Name,
            null,
            ct))
        {
            throw new BusinessException(
                $"Category '{req.Dto.Name}' already exists");
        }

        // Mapping DTO -> Entity
        var category = _mapper.Map<Category>(req.Dto);

        await _uow.Categories.AddAsync(category, ct);

        await _uow.SaveChangesAsync(ct);

        // Mapping Entity -> DTO
        return _mapper.Map<CategoryDto>(category);
    }
}