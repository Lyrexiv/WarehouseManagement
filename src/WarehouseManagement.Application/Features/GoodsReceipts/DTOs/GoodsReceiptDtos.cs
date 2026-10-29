namespace WarehouseManagement.Application.Features.GoodsReceipts;

public record GoodsReceiptDetailDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductSKU,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string? Note);

public record GoodsReceiptDto(
    Guid Id,
    string ReceiptNumber,
    Guid WarehouseId,
    string WarehouseName,
    Guid SupplierId,
    string SupplierName,
    DateTime ImportDate,
    decimal TotalAmount,
    string Status,
    string? Note,
    IEnumerable<GoodsReceiptDetailDto> Details,
    DateTime CreatedAt);

public record CreateGoodsReceiptDetailDto(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    string? Note);

public record CreateGoodsReceiptDto(
    Guid WarehouseId,
    Guid SupplierId,
    DateTime ImportDate,
    string? Note,
    IEnumerable<CreateGoodsReceiptDetailDto> Details);