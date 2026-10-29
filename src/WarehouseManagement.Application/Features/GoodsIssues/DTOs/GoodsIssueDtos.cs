namespace WarehouseManagement.Application.Features.GoodsIssues.DTOs;

// ============================================================
// GOODS ISSUE DETAIL DTO
// ============================================================

public record GoodsIssueDetailDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductSKU,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string? Note);


// ============================================================
// GOODS ISSUE DTO
// ============================================================

public record GoodsIssueDto(
    Guid Id,
    string IssueNumber,
    Guid WarehouseId,
    string WarehouseName,
    DateTime ExportDate,
    string ReceiverName,
    string? ReceiverDepartment,
    string? Purpose,
    string Status,
    string? Note,
    IEnumerable<GoodsIssueDetailDto> Details,
    DateTime CreatedAt);


// ============================================================
// CREATE GOODS ISSUE DETAIL DTO
// ============================================================

public record CreateGoodsIssueDetailDto(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    string? Note);


// ============================================================
// CREATE GOODS ISSUE DTO
// ============================================================

public record CreateGoodsIssueDto(
    Guid WarehouseId,
    DateTime ExportDate,
    string ReceiverName,
    string? ReceiverDepartment,
    string? Purpose,
    string? Note,
    IEnumerable<CreateGoodsIssueDetailDto> Details);