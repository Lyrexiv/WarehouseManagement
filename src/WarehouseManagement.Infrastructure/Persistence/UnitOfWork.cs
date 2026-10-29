using Microsoft.EntityFrameworkCore.Storage;
using WarehouseManagement.Domain.Interfaces;
using WarehouseManagement.Infrastructure.Persistence.Repositories;

namespace WarehouseManagement.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _ctx;
    private IDbContextTransaction? _tx;

    public UnitOfWork(ApplicationDbContext ctx)
    {
        _ctx = ctx;

        // Identity
        Users = new UserRepository(ctx);
        Roles = new RoleRepository(ctx);

        // Master data
        Warehouses = new WarehouseRepository(ctx);
        Products = new ProductRepository(ctx);
        Categories = new CategoryRepository(ctx);
        Suppliers = new SupplierRepository(ctx);

        // Inventory
        Inventories = new InventoryRepository(ctx);
        StockMovements = new StockMovementRepository(ctx);
        StockAdjustments = new StockAdjustmentRepository(ctx);

        // Transactions
        GoodsReceipts = new GoodsReceiptRepository(ctx);
        GoodsIssues = new GoodsIssueRepository(ctx);

        // Audit
        InventoryAudits = new InventoryAuditRepository(ctx);

        // Permissions
        WarehouseAssignments = new WarehouseAssignmentRepository(ctx);
        StaffPermissions = new StaffPermissionRepository(ctx);
    }

    // ============================================================
    // Properties
    // ============================================================

    public IUserRepository Users { get; }

    public IRoleRepository Roles { get; }

    public IWarehouseRepository Warehouses { get; }

    public IProductRepository Products { get; }

    public ICategoryRepository Categories { get; }

    public ISupplierRepository Suppliers { get; }

    public IInventoryRepository Inventories { get; }

    public IStockMovementRepository StockMovements { get; }

    public IStockAdjustmentRepository StockAdjustments { get; }

    public IGoodsReceiptRepository GoodsReceipts { get; }

    public IGoodsIssueRepository GoodsIssues { get; }

    public IInventoryAuditRepository InventoryAudits { get; }

    public IWarehouseAssignmentRepository WarehouseAssignments { get; }

    public IStaffPermissionRepository StaffPermissions { get; }

    // ============================================================
    // Save
    // ============================================================

    public Task<int> SaveChangesAsync(
        CancellationToken ct = default)
        => _ctx.SaveChangesAsync(ct);

    // ============================================================
    // Transaction
    // ============================================================

    public async Task BeginTransactionAsync(
        CancellationToken ct = default)
    {
        _tx = await _ctx.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(
        CancellationToken ct = default)
    {
        if (_tx == null)
            return;

        await _tx.CommitAsync(ct);
        await _tx.DisposeAsync();

        _tx = null;
    }

    public async Task RollbackTransactionAsync(
        CancellationToken ct = default)
    {
        if (_tx == null)
            return;

        await _tx.RollbackAsync(ct);
        await _tx.DisposeAsync();

        _tx = null;
    }

    public void Dispose()
    {
        _tx?.Dispose();
        _ctx.Dispose();
    }
}