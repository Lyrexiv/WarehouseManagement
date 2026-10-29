using MediatR;
using WarehouseManagement.Application.Common.Interfaces;
using WarehouseManagement.Application.Features.Dashboard.DTOs;
using WarehouseManagement.Domain.Exceptions;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Application.Features.Dashboard.Queries;

public class GetDashboardQueryHandler
    : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _me;
    private readonly IPermissionService _perm;

    public GetDashboardQueryHandler(
        IUnitOfWork uow,
        ICurrentUserService me,
        IPermissionService perm)
    {
        _uow = uow;
        _me = me;
        _perm = perm;
    }

    public async Task<DashboardDto> Handle(
        GetDashboardQuery request,
        CancellationToken ct)
    {
        // ============================================================
        // 1. Get warehouses accessible by current user
        // ============================================================

        IReadOnlyList<Guid>? accessibleIds = null;

        if (!_me.IsAdmin)
        {
            var userId = _me.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException(
                    "User is not authenticated");
            }

            accessibleIds =
                await _perm.GetAccessibleWarehouseIdsAsync(
                    userId.Value,
                    ct);
        }

        // ============================================================
        // 2. Load master data
        // ============================================================

        var products =
            await _uow.Products.GetAllAsync(ct);

        var warehouses =
            await _uow.Warehouses.GetAllAsync(ct);

        var suppliers =
            await _uow.Suppliers.GetAllAsync(ct);

        // ============================================================
        // 3. Filter warehouses by permission
        // ============================================================

        var filteredWarehouses = accessibleIds == null
            ? warehouses.Where(w =>
                !w.IsDeleted &&
                w.IsActive)

            : warehouses.Where(w =>
                !w.IsDeleted &&
                w.IsActive &&
                accessibleIds.Contains(w.Id));

        // ============================================================
        // 4. Low stock
        // ============================================================

        var lowStock =
            await _uow.Inventories.GetLowStockAsync(ct);

        if (accessibleIds != null)
        {
            lowStock = lowStock
                .Where(i =>
                    accessibleIds.Contains(i.WarehouseId))
                .ToList();
        }

        // ============================================================
        // 5. Goods receipts / issues this month
        // ============================================================

        var receiptCount =
            await _uow.GoodsReceipts
                .GetCountThisMonthAsync(ct);

        var issueCount =
            await _uow.GoodsIssues
                .GetCountThisMonthAsync(ct);

        // ============================================================
        // 6. Total inventory value
        // ============================================================

        var totalInventoryValue =
            await _uow.Inventories
                .GetTotalInventoryValueAsync(ct);

        // ============================================================
        // 7. Recent stock movements
        // ============================================================

        var movements =
            await _uow.StockMovements.GetAllAsync(ct);

        var filteredMovements = accessibleIds == null
            ? movements
            : movements.Where(m =>
                accessibleIds.Contains(m.WarehouseId));

        var recentMovements = filteredMovements
            .OrderByDescending(m => m.CreatedAt)
            .Take(10)
            .Select(m => new RecentMovementDto(
                m.ProductId,
                m.Product?.Name ?? string.Empty,
                m.WarehouseId,
                m.Warehouse?.Name ?? string.Empty,
                m.MovementType.ToString(),
                m.Quantity,
                m.ReferenceNumber,
                m.CreatedAt))
            .ToList();

        // ============================================================
        // 8. Low stock DTO
        // ============================================================

        var lowStockItems = lowStock
            .Select(i => new LowStockItemDto(
                i.ProductId,
                i.Product?.Name ?? string.Empty,
                i.Product?.SKU ?? string.Empty,
                i.WarehouseId,
                i.Warehouse?.Name ?? string.Empty,
                i.Quantity,
                i.Product?.MinimumStock ?? 0,
                (i.Product?.MinimumStock ?? 0) - i.Quantity))
            .ToList();

        // ============================================================
        // 9. Dashboard result
        // ============================================================

        return new DashboardDto(
            TotalProducts: products.Count(p =>
                !p.IsDeleted &&
                p.IsActive),

            TotalWarehouses:
                filteredWarehouses.Count(),

            TotalSuppliers: suppliers.Count(s =>
                !s.IsDeleted &&
                s.IsActive),

            LowStockCount:
                lowStock.Count,

            GoodsReceiptsThisMonth:
                receiptCount,

            GoodsIssuesThisMonth:
                issueCount,

            TotalInventoryValue:
                totalInventoryValue,

            LowStockItems:
                lowStockItems,

            RecentMovements:
                recentMovements);
    }
}