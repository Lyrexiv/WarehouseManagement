using AutoMapper;
using MediatR;
using WarehouseManagement.Application.Features.Categories.DTOs;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.Categories.Queries;

public class GetCategoryByIdQueryHandler
    : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetCategoryByIdQueryHandler(
        IUnitOfWork uow,
        IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<CategoryDto> Handle(
        GetCategoryByIdQuery req,
        CancellationToken ct)
    {
        var category = await _uow.Categories.GetByIdAsync(
            req.Id, ct)
            ?? throw new NotFoundException("Category", req.Id);

        return _mapper.Map<CategoryDto>(category);
    }
}