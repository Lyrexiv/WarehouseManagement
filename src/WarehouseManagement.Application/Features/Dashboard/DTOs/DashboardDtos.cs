namespace WarehouseManagement.Application.Features.Dashboard.DTOs;

public record DashboardDto(
    int TotalProducts,
    int TotalWarehouses,
    int TotalSuppliers,
    int LowStockCount,
    int GoodsReceiptsThisMonth,
    int GoodsIssuesThisMonth,
    decimal TotalInventoryValue,
    IEnumerable<LowStockItemDto> LowStockItems,
    IEnumerable<RecentMovementDto> RecentMovements);

public record LowStockItemDto(
    Guid ProductId,
    string ProductName,
    string SKU,
    Guid WarehouseId,
    string WarehouseName,
    int CurrentStock,
    int MinimumStock,
    int Shortage);

public record RecentMovementDto(
    Guid ProductId,
    string ProductName,
    Guid WarehouseId,
    string WarehouseName,
    string MovementType,
    int Quantity,
    string? ReferenceNumber,
    DateTime CreatedAt);