using MediatR;

using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Domain.Entities;
using WarehouseManagement.Domain.Enums;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.Inventories;

// ================================================================
// DTO
// ================================================================

public record InventoryDto(
    Guid Id,
    Guid WarehouseId,
    string WarehouseName,
    Guid ProductId,
    string ProductName,
    string ProductSKU,
    string Unit,
    int Quantity,
    int ReservedQuantity,
    int AvailableQuantity,
    bool IsLowStock,
    int MinimumStock,
    decimal UnitPrice,
    decimal TotalValue);


// ================================================================
// QUERIES
// ================================================================

public record GetInventoryByWarehouseQuery(
    Guid WarehouseId)
    : IRequest<IEnumerable<InventoryDto>>;

public record GetInventoryByProductQuery(
    Guid ProductId)
    : IRequest<IEnumerable<InventoryDto>>;

public record GetLowStockQuery()
    : IRequest<IEnumerable<InventoryDto>>;


// ================================================================
// GET INVENTORY BY WAREHOUSE
// ================================================================

public class GetInventoryByWarehouseQueryHandler
    : IRequestHandler<
        GetInventoryByWarehouseQuery,
        IEnumerable<InventoryDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _me;
    private readonly IPermissionService _perm;

    public GetInventoryByWarehouseQueryHandler(
        IUnitOfWork uow,
        ICurrentUserService me,
        IPermissionService perm)
    {
        _uow = uow;
        _me = me;
        _perm = perm;
    }

    public async Task<IEnumerable<InventoryDto>> Handle(
        GetInventoryByWarehouseQuery req,
        CancellationToken ct)
    {
        // =========================================================
        // KIỂM TRA USER ĐĂNG NHẬP
        // =========================================================

        var userId = _me.UserId;

        if (userId is null)
        {
            throw new UnauthorizedException(
                "User is not authenticated.");
        }

        // =========================================================
        // KIỂM TRA QUYỀN VIEW INVENTORY
        // =========================================================

        if (!await _perm.HasStaffPermissionAsync(
                userId.Value,
                StaffPermission.ViewInventory,
                req.WarehouseId,
                ct))
        {
            throw new ForbiddenException(
                "Bạn không có quyền xem tồn kho kho này.");
        }

        // =========================================================
        // KIỂM TRA QUYỀN TRUY CẬP WAREHOUSE
        // =========================================================

        if (!await _perm.CanAccessWarehouseAsync(
                userId.Value,
                req.WarehouseId,
                ct))
        {
            throw new ForbiddenException(
                "Bạn không được phân công quản lý kho này.");
        }

        // =========================================================
        // LẤY INVENTORY
        // =========================================================

        var inventories =
            await _uow.Inventories.GetByWarehouseAsync(
                req.WarehouseId,
                ct);

        // =========================================================
        // MAP DTO
        // =========================================================

        return inventories
            .Select(MapToDto)
            .ToList();
    }

    private static InventoryDto MapToDto(
        Inventory i)
    {
        var unitPrice =
            i.Product?.UnitPrice ?? 0;

        var minimumStock =
            i.Product?.MinimumStock ?? 0;

        return new InventoryDto(
            i.Id,

            i.WarehouseId,
            i.Warehouse?.Name ?? string.Empty,

            i.ProductId,
            i.Product?.Name ?? string.Empty,
            i.Product?.SKU ?? string.Empty,
            i.Product?.Unit ?? string.Empty,

            i.Quantity,
            i.ReservedQuantity,
            i.AvailableQuantity,

            i.Product != null &&
            i.Quantity <= i.Product.MinimumStock,

            minimumStock,
            unitPrice,

            i.Quantity * unitPrice
        );
    }
}


// ================================================================
// GET INVENTORY BY PRODUCT
// ================================================================

public class GetInventoryByProductQueryHandler
    : IRequestHandler<
        GetInventoryByProductQuery,
        IEnumerable<InventoryDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _me;
    private readonly IPermissionService _perm;

    public GetInventoryByProductQueryHandler(
        IUnitOfWork uow,
        ICurrentUserService me,
        IPermissionService perm)
    {
        _uow = uow;
        _me = me;
        _perm = perm;
    }

    public async Task<IEnumerable<InventoryDto>> Handle(
        GetInventoryByProductQuery req,
        CancellationToken ct)
    {
        // =========================================================
        // KIỂM TRA USER ĐĂNG NHẬP
        // =========================================================

        var userId = _me.UserId;

        if (userId is null)
        {
            throw new UnauthorizedException(
                "User is not authenticated.");
        }

        // =========================================================
        // LẤY INVENTORY THEO PRODUCT
        // =========================================================

        var inventories =
            await _uow.Inventories.GetByProductAsync(
                req.ProductId,
                ct);

        // =========================================================
        // ADMIN
        // =========================================================

        if (!_me.IsAdmin)
        {
            // =====================================================
            // MANAGER / STAFF
            // Chỉ lấy inventory thuộc warehouse có quyền
            // =====================================================

            var accessibleWarehouseIds =
                await _perm.GetAccessibleWarehouseIdsAsync(
                    userId.Value,
                    ct);

            if (accessibleWarehouseIds != null)
            {
                inventories =
                    inventories
                        .Where(i =>
                            accessibleWarehouseIds.Contains(
                                i.WarehouseId))
                        .ToList();
            }
        }

        // =========================================================
        // MAP DTO
        // =========================================================

        return inventories
            .Select(MapToDto)
            .ToList();
    }

    private static InventoryDto MapToDto(
        Inventory i)
    {
        var unitPrice =
            i.Product?.UnitPrice ?? 0;

        var minimumStock =
            i.Product?.MinimumStock ?? 0;

        return new InventoryDto(
            i.Id,

            i.WarehouseId,
            i.Warehouse?.Name ?? string.Empty,

            i.ProductId,
            i.Product?.Name ?? string.Empty,
            i.Product?.SKU ?? string.Empty,
            i.Product?.Unit ?? string.Empty,

            i.Quantity,
            i.ReservedQuantity,
            i.AvailableQuantity,

            i.Product != null &&
            i.Quantity <= i.Product.MinimumStock,

            minimumStock,
            unitPrice,

            i.Quantity * unitPrice
        );
    }
}


// ================================================================
// GET LOW STOCK
// ================================================================

public class GetLowStockQueryHandler
    : IRequestHandler<
        GetLowStockQuery,
        IEnumerable<InventoryDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _me;
    private readonly IPermissionService _perm;

    public GetLowStockQueryHandler(
        IUnitOfWork uow,
        ICurrentUserService me,
        IPermissionService perm)
    {
        _uow = uow;
        _me = me;
        _perm = perm;
    }

    public async Task<IEnumerable<InventoryDto>> Handle(
        GetLowStockQuery req,
        CancellationToken ct)
    {
        // =========================================================
        // KIỂM TRA USER ĐĂNG NHẬP
        // =========================================================

        var userId = _me.UserId;

        if (userId is null)
        {
            throw new UnauthorizedException(
                "User is not authenticated.");
        }

        // =========================================================
        // LẤY LOW STOCK
        // =========================================================

        var inventories =
            await _uow.Inventories.GetLowStockAsync(ct);

        // =========================================================
        // ADMIN
        // =========================================================

        if (!_me.IsAdmin)
        {
            // =====================================================
            // MANAGER / STAFF
            // Chỉ lấy low stock của warehouse có quyền
            // =====================================================

            var accessibleWarehouseIds =
                await _perm.GetAccessibleWarehouseIdsAsync(
                    userId.Value,
                    ct);

            if (accessibleWarehouseIds != null)
            {
                inventories =
                    inventories
                        .Where(i =>
                            accessibleWarehouseIds.Contains(
                                i.WarehouseId))
                        .ToList();
            }
        }

        // =========================================================
        // MAP DTO
        // =========================================================

        return inventories
            .Select(i =>
                new InventoryDto(
                    i.Id,

                    i.WarehouseId,
                    i.Warehouse?.Name ?? string.Empty,

                    i.ProductId,
                    i.Product?.Name ?? string.Empty,
                    i.Product?.SKU ?? string.Empty,
                    i.Product?.Unit ?? string.Empty,

                    i.Quantity,
                    i.ReservedQuantity,
                    i.AvailableQuantity,

                    true,

                    i.Product?.MinimumStock ?? 0,
                    i.Product?.UnitPrice ?? 0,

                    i.Quantity *
                    (i.Product?.UnitPrice ?? 0)
                ))
            .ToList();
    }
}