using AutoMapper;
using FluentValidation;
using MediatR;
using WarehouseManagement.Application.Features.Products.DTOs;
using WarehouseManagement.Domain.Entities;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.Products.Commands;

public record CreateProductCommand(CreateProductDto Dto)
    : IRequest<ProductDto>;

// Validator
public class CreateProductCommandValidator
    : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Dto.SKU)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Dto.Barcode)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Dto.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Dto.Unit)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.Dto.UnitPrice)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Dto.MinimumStock)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Dto.CategoryId)
            .NotEmpty();
    }
}

// Handler
public class CreateProductCommandHandler
    : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateProductCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        // Check SKU
        if (await _unitOfWork.Products.SkuExistsAsync(
            dto.SKU, null, cancellationToken))
        {
            throw new DuplicateCodeException("Product", dto.SKU);
        }

        // Check Barcode
        if (await _unitOfWork.Products.BarcodeExistsAsync(
            dto.Barcode, null, cancellationToken))
        {
            throw new BusinessException(
                $"Barcode '{dto.Barcode}' already exists");
        }

        // Check Category
        var category = await _unitOfWork.Categories.GetByIdAsync(
            dto.CategoryId, cancellationToken)
            ?? throw new NotFoundException(
                "Category", dto.CategoryId);

        // Mapping DTO -> Entity
        var product = _mapper.Map<Product>(dto);

        // Assign navigation property for ProductDto mapping
        product.Category = category;

        await _unitOfWork.Products.AddAsync(
            product, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Mapping Entity -> DTO
        return _mapper.Map<ProductDto>(product);
    }
}