using FluentValidation;
using MediatR;
using WarehouseManagement.Application.Common.Models;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.Products;

// ============ DTOs ============
public record ProductDto(
    Guid Id,
    string SKU,
    string Barcode,
    string Name,
    string? Description,
    string Unit,
    decimal UnitPrice,
    int MinimumStock,
    bool IsActive,
    Guid CategoryId,
    string CategoryName,
    DateTime CreatedAt
);

public record CreateProductDto(
    string SKU,
    string Barcode,
    string Name,
    string? Description,
    string Unit,
    decimal UnitPrice,
    int MinimumStock,
    Guid CategoryId
);

public record UpdateProductDto(
    string SKU,
    string Barcode,
    string Name,
    string? Description,
    string Unit,
    decimal UnitPrice,
    int MinimumStock,
    bool IsActive,
    Guid CategoryId
);

// ============ COMMANDS ============
public record CreateProductCommand(CreateProductDto Dto) : IRequest<ProductDto>;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Dto.SKU).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Dto.Barcode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Dto.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Dto.Unit).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Dto.UnitPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Dto.MinimumStock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Dto.CategoryId).NotEmpty();
    }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        if (await _unitOfWork.Products.SkuExistsAsync(dto.SKU, null, cancellationToken))
            throw new DuplicateCodeException("Product", dto.SKU);

        if (await _unitOfWork.Products.BarcodeExistsAsync(dto.Barcode, null, cancellationToken))
            throw new BusinessException($"Barcode '{dto.Barcode}' already exists");

        var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId, cancellationToken)
            ?? throw new NotFoundException("Category", dto.CategoryId);

        var product = new Domain.Entities.Product
        {
            SKU = dto.SKU,
            Barcode = dto.Barcode,
            Name = dto.Name,
            Description = dto.Description,
            Unit = dto.Unit,
            UnitPrice = dto.UnitPrice,
            MinimumStock = dto.MinimumStock,
            CategoryId = dto.CategoryId
        };

        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ProductDto(product.Id, product.SKU, product.Barcode, product.Name, product.Description,
            product.Unit, product.UnitPrice, product.MinimumStock, product.IsActive,
            product.CategoryId, category.Name, product.CreatedAt);
    }
}

public record UpdateProductCommand(Guid Id, UpdateProductDto Dto) : IRequest<ProductDto>;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Product", request.Id);

        var dto = request.Dto;

        if (await _unitOfWork.Products.SkuExistsAsync(dto.SKU, request.Id, cancellationToken))
            throw new DuplicateCodeException("Product", dto.SKU);

        if (await _unitOfWork.Products.BarcodeExistsAsync(dto.Barcode, request.Id, cancellationToken))
            throw new BusinessException($"Barcode '{dto.Barcode}' already exists");

        var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId, cancellationToken)
            ?? throw new NotFoundException("Category", dto.CategoryId);

        product.SKU = dto.SKU;
        product.Barcode = dto.Barcode;
        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Unit = dto.Unit;
        product.UnitPrice = dto.UnitPrice;
        product.MinimumStock = dto.MinimumStock;
        product.IsActive = dto.IsActive;
        product.CategoryId = dto.CategoryId;
        product.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ProductDto(product.Id, product.SKU, product.Barcode, product.Name, product.Description,
            product.Unit, product.UnitPrice, product.MinimumStock, product.IsActive,
            product.CategoryId, category.Name, product.CreatedAt);
    }
}

public record DeleteProductCommand(Guid Id) : IRequest<bool>;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Product", request.Id);

        product.IsDeleted = true;
        product.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}

// ============ QUERIES ============
public record GetAllProductsQuery(PaginationParams Params) : IRequest<PagedList<ProductDto>>;

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, PagedList<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllProductsQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<PagedList<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.Products.GetWithCategoryAsync(cancellationToken);
        var query = products.Where(p => !p.IsDeleted).AsQueryable();

        if (!string.IsNullOrEmpty(request.Params.SearchTerm))
        {
            var term = request.Params.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.SKU.ToLower().Contains(term) ||
                p.Barcode.ToLower().Contains(term));
        }

        var total = query.Count();
        var items = query
            .Skip((request.Params.PageNumber - 1) * request.Params.PageSize)
            .Take(request.Params.PageSize)
            .Select(p => new ProductDto(p.Id, p.SKU, p.Barcode, p.Name, p.Description,
                p.Unit, p.UnitPrice, p.MinimumStock, p.IsActive, p.CategoryId,
                p.Category.Name, p.CreatedAt))
            .ToList();

        return new PagedList<ProductDto>(items, total, request.Params.PageNumber, request.Params.PageSize);
    }
}

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProductByIdQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetWithCategoryByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Product", request.Id);

        return new ProductDto(product.Id, product.SKU, product.Barcode, product.Name, product.Description,
            product.Unit, product.UnitPrice, product.MinimumStock, product.IsActive,
            product.CategoryId, product.Category.Name, product.CreatedAt);
    }
}