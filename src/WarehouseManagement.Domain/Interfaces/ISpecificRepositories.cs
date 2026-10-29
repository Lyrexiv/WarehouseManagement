using WarehouseManagement.Domain.Entities;
namespace WarehouseManagement.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetWithRolesAsync(Guid userId, CancellationToken ct = default);
    Task<User?> GetByRefreshTokenHashAsync(string refreshTokenHash,CancellationToken ct);
    Task<IReadOnlyList<User>> GetAllWithRolesAsync(CancellationToken ct = default);
}
public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetByNameAsync(string name, CancellationToken ct = default);
}
public interface IWarehouseRepository : IRepository<Warehouse>
{
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default);
}
public interface IProductRepository : IRepository<Product>
{
    Task<bool> SkuExistsAsync(string sku, Guid? excludeId = null, CancellationToken ct = default);
    Task<bool> BarcodeExistsAsync(string barcode, Guid? excludeId = null, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetWithCategoryAsync(CancellationToken ct = default);
    Task<Product?> GetWithCategoryByIdAsync(Guid id, CancellationToken ct = default);
}
public interface ICategoryRepository : IRepository<Category>
{
    Task<bool> NameExistsAsync(string name, Guid? excludeId = null, CancellationToken ct = default);
}
public interface ISupplierRepository : IRepository<Supplier>
{
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default);
}
public interface IInventoryRepository : IRepository<Inventory>
{
    Task<Inventory?> GetByWarehouseAndProductAsync(Guid warehouseId, Guid productId, CancellationToken ct = default);
    Task<IReadOnlyList<Inventory>> GetByWarehouseAsync(Guid warehouseId, CancellationToken ct = default);
    Task<IReadOnlyList<Inventory>> GetByProductAsync(Guid productId, CancellationToken ct = default);
    Task<IReadOnlyList<Inventory>> GetLowStockAsync(CancellationToken ct = default);
    Task<decimal> GetTotalInventoryValueAsync(CancellationToken ct = default);
}
public interface IStockMovementRepository : IRepository<StockMovement>
{
    Task<IReadOnlyList<StockMovement>> GetByProductAsync(Guid productId, CancellationToken ct = default);
    Task<IReadOnlyList<StockMovement>> GetByWarehouseAsync(Guid warehouseId, CancellationToken ct = default);
}
public interface IGoodsReceiptRepository : IRepository<GoodsReceipt>
{
    Task<GoodsReceipt?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<bool> NumberExistsAsync(string number, CancellationToken ct = default);
    Task<int> GetCountThisMonthAsync(CancellationToken ct = default);
    Task<IReadOnlyList<GoodsReceipt>> GetAllWithDetailsAsync(CancellationToken ct = default);
}
public interface IGoodsIssueRepository : IRepository<GoodsIssue>
{
    Task<GoodsIssue?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<bool> NumberExistsAsync(string number, CancellationToken ct = default);
    Task<int> GetCountThisMonthAsync(CancellationToken ct = default);
    Task<IReadOnlyList<GoodsIssue>> GetAllWithDetailsAsync(CancellationToken ct = default);
}
public interface IStockAdjustmentRepository : IRepository<StockAdjustment>
{
    Task<bool> NumberExistsAsync(string number, CancellationToken ct = default);
}
public interface IInventoryAuditRepository : IRepository<InventoryAudit>
{
    Task<InventoryAudit?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<bool> NumberExistsAsync(string number, CancellationToken ct = default);
}
