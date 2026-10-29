using WarehouseManagement.Domain.Entities;

namespace WarehouseManagement.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // ---- Core ----
    IUserRepository Users { get; }
    IRoleRepository Roles { get; }
    IWarehouseRepository Warehouses { get; }
    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }
    ISupplierRepository Suppliers { get; }

    // ---- Inventory ----
    IInventoryRepository Inventories { get; }
    IStockMovementRepository StockMovements { get; }
    IStockAdjustmentRepository StockAdjustments { get; }

    // ---- Transactions ----
    IGoodsReceiptRepository GoodsReceipts { get; }
    IGoodsIssueRepository GoodsIssues { get; }

    // ---- Audit ----
    IInventoryAuditRepository InventoryAudits { get; }

    // ---- Permissions (NEW) ----
    IWarehouseAssignmentRepository WarehouseAssignments { get; }
    IStaffPermissionRepository StaffPermissions { get; }

    // ---- Transactions ----
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}