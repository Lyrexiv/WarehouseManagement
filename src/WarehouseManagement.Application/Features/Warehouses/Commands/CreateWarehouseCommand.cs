using AutoMapper;
using FluentValidation;
using MediatR;
using WarehouseManagement.Domain.Entities;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;
using WarehouseManagement.Application.Features.Warehouses.DTOs;

namespace WarehouseManagement.Application.Features.Warehouses.Commands;

public record CreateWarehouseCommand(CreateWarehouseDto Dto)
    : IRequest<WarehouseDto>;

public class CreateWarehouseCommandValidator
    : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator()
    {
        RuleFor(x => x.Dto.Code)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.Dto.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Dto.Address)
            .NotEmpty();

        RuleFor(x => x.Dto.Capacity)
            .GreaterThan(0);
    }
}

public class CreateWarehouseCommandHandler
    : IRequestHandler<CreateWarehouseCommand, WarehouseDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CreateWarehouseCommandHandler(
        IUnitOfWork uow,
        IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<WarehouseDto> Handle(
        CreateWarehouseCommand request,
        CancellationToken cancellationToken)
    {
        if (await _uow.Warehouses.CodeExistsAsync(
            request.Dto.Code,
            null,
            cancellationToken))
        {
            throw new DuplicateCodeException(
                "Warehouse",
                request.Dto.Code);
        }

        var warehouse = _mapper.Map<Warehouse>(request.Dto);

        await _uow.Warehouses.AddAsync(
            warehouse,
            cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WarehouseDto>(warehouse);
    }
}