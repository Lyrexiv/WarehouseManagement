using AutoMapper;
using FluentValidation;
using MediatR;
using WarehouseManagement.Application.Features.Products.DTOs;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.Products.Commands;

public record UpdateProductCommand(Guid Id, UpdateProductDto Dto)
    : IRequest<ProductDto>;

// Validator
public class UpdateProductCommandValidator
    : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

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
public class UpdateProductCommandHandler
    : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateProductCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        // Get existing product
        var product = await _unitOfWork.Products.GetByIdAsync(
            request.Id, cancellationToken)
            ?? throw new NotFoundException(
                "Product", request.Id);

        var dto = request.Dto;

        // Check SKU
        if (await _unitOfWork.Products.SkuExistsAsync(
            dto.SKU, request.Id, cancellationToken))
        {
            throw new DuplicateCodeException("Product", dto.SKU);
        }

        // Check Barcode
        if (await _unitOfWork.Products.BarcodeExistsAsync(
            dto.Barcode, request.Id, cancellationToken))
        {
            throw new BusinessException(
                $"Barcode '{dto.Barcode}' already exists");
        }

        // Check Category
        var category = await _unitOfWork.Categories.GetByIdAsync(
            dto.CategoryId, cancellationToken)
            ?? throw new NotFoundException(
                "Category", dto.CategoryId);

        // Mapping DTO -> existing Entity
        _mapper.Map(dto, product);

        // Update navigation property
        product.Category = category;

        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(
            product, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Mapping Entity -> DTO
        return _mapper.Map<ProductDto>(product);
    }
}