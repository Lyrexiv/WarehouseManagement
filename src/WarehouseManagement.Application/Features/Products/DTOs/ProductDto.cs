namespace WarehouseManagement.Application.Features.Products.DTOs;

// Entity -> DTO
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

// Request tạo sản phẩm
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

// Request cập nhật sản phẩm
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