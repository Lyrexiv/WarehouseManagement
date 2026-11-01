using AutoMapper;
using FluentValidation;
using MediatR;
using WarehouseManagement.Domain.Entities;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;
using WarehouseManagement.Application.Features.Suppliers.DTOs;

namespace WarehouseManagement.Application.Features.Suppliers.Commands;

// Command
public record CreateSupplierCommand(CreateSupplierDto Dto)
    : IRequest<SupplierDto>;

// Validator
public class CreateSupplierCommandValidator
    : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierCommandValidator()
    {
        RuleFor(x => x.Dto.Code)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.Dto.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Dto.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrEmpty(x.Dto.Email));
    }
}

// Handler
public class CreateSupplierCommandHandler
    : IRequestHandler<CreateSupplierCommand, SupplierDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CreateSupplierCommandHandler(
        IUnitOfWork uow,
        IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<SupplierDto> Handle(
        CreateSupplierCommand request,
        CancellationToken cancellationToken)
    {
        if (await _uow.Suppliers.CodeExistsAsync(
            request.Dto.Code, null, cancellationToken))
        {
            throw new DuplicateCodeException(
                "Supplier",
                request.Dto.Code);
        }

        var supplier = _mapper.Map<Supplier>(request.Dto);

        await _uow.Suppliers.AddAsync(supplier, cancellationToken);

        await _uow.SaveChangesAsync(cancellationToken);

        return _mapper.Map<SupplierDto>(supplier);
    }
}