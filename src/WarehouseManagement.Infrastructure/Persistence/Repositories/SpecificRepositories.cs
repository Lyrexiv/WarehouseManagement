using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Domain.Entities;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Infrastructure.Persistence.Repositories;


public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(ApplicationDbContext ctx) : base(ctx) { }
    public async Task<Role?> GetByNameAsync(string name, CancellationToken ct = default)
        => await _db.FirstOrDefaultAsync(r => r.Name == name, ct);
}

public class WarehouseRepository : Repository<Warehouse>, IWarehouseRepository
{
    public WarehouseRepository(ApplicationDbContext ctx) : base(ctx) { }
    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default)
        => await _db.AnyAsync(w => w.Code == code && (excludeId == null || w.Id != excludeId), ct);
}

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext ctx) : base(ctx) { }
    public async Task<bool> SkuExistsAsync(string sku, Guid? excludeId = null, CancellationToken ct = default)
        => await _db.AnyAsync(p => p.SKU == sku && (excludeId == null || p.Id != excludeId), ct);
    public async Task<bool> BarcodeExistsAsync(string barcode, Guid? excludeId = null, CancellationToken ct = default)
        => await _db.AnyAsync(p => p.Barcode == barcode && (excludeId == null || p.Id != excludeId), ct);
    public async Task<IReadOnlyList<Product>> GetWithCategoryAsync(CancellationToken ct = default)
        => await _db.Include(p => p.Category).ToListAsync(ct);
    public async Task<Product?> GetWithCategoryByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id, ct);
}

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext ctx) : base(ctx) { }
    public async Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken ct = default)
        => await _db.AnyAsync(c => c.Name == name && (excludeId == null || c.Id != excludeId), ct);
}

public class SupplierRepository : Repository<Supplier>, ISupplierRepository
{
    public SupplierRepository(ApplicationDbContext ctx) : base(ctx) { }
    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default)
        => await _db.AnyAsync(s => s.Code == code && (excludeId == null || s.Id != excludeId), ct);
}

public class InventoryRepository : Repository<Inventory>, IInventoryRepository
{
    public InventoryRepository(ApplicationDbContext ctx) : base(ctx) { }
    public async Task<Inventory?> GetByWarehouseAndProductAsync(Guid wId, Guid pId, CancellationToken ct = default)
        => await _db.Include(i => i.Product).Include(i => i.Warehouse)
            .FirstOrDefaultAsync(i => i.WarehouseId == wId && i.ProductId == pId, ct);
    public async Task<IReadOnlyList<Inventory>> GetByWarehouseAsync(Guid wId, CancellationToken ct = default)
        => await _db.Include(i => i.Product).ThenInclude(p => p.Category).Include(i => i.Warehouse)
            .Where(i => i.WarehouseId == wId).ToListAsync(ct);
    public async Task<IReadOnlyList<Inventory>> GetByProductAsync(Guid pId, CancellationToken ct = default)
        => await _db.Include(i => i.Warehouse).Include(i => i.Product)
            .Where(i => i.ProductId == pId).ToListAsync(ct);
    public async Task<IReadOnlyList<Inventory>> GetLowStockAsync(CancellationToken ct = default)
        => await _db.Include(i => i.Product).Include(i => i.Warehouse)
            .Where(i => i.Quantity <= i.Product.MinimumStock).ToListAsync(ct);
    public async Task<decimal> GetTotalInventoryValueAsync(CancellationToken ct = default)
        => await _db.Include(i => i.Product).SumAsync(i => i.Quantity * i.Product.UnitPrice, ct);
}

public class StockMovementRepository : Repository<StockMovement>, IStockMovementRepository
{
    public StockMovementRepository(ApplicationDbContext ctx) : base(ctx) { }
    public async Task<IReadOnlyList<StockMovement>> GetByProductAsync(Guid pId, CancellationToken ct = default)
        => await _db.Where(s => s.ProductId == pId).OrderByDescending(s => s.CreatedAt).ToListAsync(ct);
    public async Task<IReadOnlyList<StockMovement>> GetByWarehouseAsync(Guid wId, CancellationToken ct = default)
        => await _db.Where(s => s.WarehouseId == wId).OrderByDescending(s => s.CreatedAt).ToListAsync(ct);
}

public class GoodsReceiptRepository : Repository<GoodsReceipt>, IGoodsReceiptRepository
{
    public GoodsReceiptRepository(ApplicationDbContext ctx) : base(ctx) { }
    public async Task<GoodsReceipt?> GetWithDetailsAsync(Guid id, CancellationToken ct = default)
        => await _db.Include(g => g.Warehouse).Include(g => g.Supplier)
            .Include(g => g.Details).ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(g => g.Id == id, ct);
    public async Task<bool> NumberExistsAsync(string number, CancellationToken ct = default)
        => await _db.AnyAsync(g => g.ReceiptNumber == number, ct);
    public async Task<int> GetCountThisMonthAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await _db.CountAsync(g => g.ImportDate.Year == now.Year && g.ImportDate.Month == now.Month, ct);
    }
    public async Task<IReadOnlyList<GoodsReceipt>> GetAllWithDetailsAsync(CancellationToken ct = default)
        => await _db.Include(g => g.Warehouse).Include(g => g.Supplier)
            .Include(g => g.Details).ThenInclude(d => d.Product)
            .OrderByDescending(g => g.CreatedAt).ToListAsync(ct);
}

public class GoodsIssueRepository : Repository<GoodsIssue>, IGoodsIssueRepository
{
    public GoodsIssueRepository(ApplicationDbContext ctx) : base(ctx) { }
    public async Task<GoodsIssue?> GetWithDetailsAsync(Guid id, CancellationToken ct = default)
        => await _db.Include(g => g.Warehouse)
            .Include(g => g.Details).ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(g => g.Id == id, ct);
    public async Task<bool> NumberExistsAsync(string number, CancellationToken ct = default)
        => await _db.AnyAsync(g => g.IssueNumber == number, ct);
    public async Task<int> GetCountThisMonthAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await _db.CountAsync(g => g.ExportDate.Year == now.Year && g.ExportDate.Month == now.Month, ct);
    }
    public async Task<IReadOnlyList<GoodsIssue>> GetAllWithDetailsAsync(CancellationToken ct = default)
        => await _db.Include(g => g.Warehouse)
            .Include(g => g.Details).ThenInclude(d => d.Product)
            .OrderByDescending(g => g.CreatedAt).ToListAsync(ct);
}

public class StockAdjustmentRepository : Repository<StockAdjustment>, IStockAdjustmentRepository
{
    public StockAdjustmentRepository(ApplicationDbContext ctx) : base(ctx) { }
    public async Task<bool> NumberExistsAsync(string number, CancellationToken ct = default)
        => await _db.AnyAsync(s => s.AdjustmentNumber == number, ct);
}

public class InventoryAuditRepository : Repository<InventoryAudit>, IInventoryAuditRepository
{
    public InventoryAuditRepository(ApplicationDbContext ctx) : base(ctx) { }
    public async Task<InventoryAudit?> GetWithDetailsAsync(Guid id, CancellationToken ct = default)
        => await _db.Include(a => a.Warehouse)
            .Include(a => a.Details).ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
    public async Task<bool> NumberExistsAsync(string number, CancellationToken ct = default)
        => await _db.AnyAsync(a => a.AuditNumber == number, ct);
}
