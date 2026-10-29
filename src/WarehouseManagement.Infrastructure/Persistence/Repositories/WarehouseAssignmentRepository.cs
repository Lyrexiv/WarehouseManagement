using Microsoft.EntityFrameworkCore;
using WarehouseManagement.Domain.Entities;
using WarehouseManagement.Domain.Interfaces;

namespace WarehouseManagement.Infrastructure.Persistence.Repositories;

// ================================================================
// WAREHOUSE ASSIGNMENT REPOSITORY
// ================================================================

public class WarehouseAssignmentRepository
    : Repository<WarehouseAssignment>, IWarehouseAssignmentRepository
{
    public WarehouseAssignmentRepository(ApplicationDbContext ctx)
        : base(ctx)
    {
    }

    public async Task<IReadOnlyList<WarehouseAssignment>> GetByUserAsync(
        Guid userId,
        CancellationToken ct = default)
        => await _db
            .Include(x => x.Warehouse)
            .Where(x =>
                x.UserId == userId &&
                x.IsActive &&
                !x.IsDeleted)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<WarehouseAssignment>> GetByWarehouseAsync(
        Guid warehouseId,
        CancellationToken ct = default)
        => await _db
            .Include(x => x.User)
            .Where(x =>
                x.WarehouseId == warehouseId &&
                x.IsActive &&
                !x.IsDeleted)
            .ToListAsync(ct);

    public async Task<bool> IsAssignedAsync(
        Guid userId,
        Guid warehouseId,
        CancellationToken ct = default)
        => await _db.AnyAsync(x =>
            x.UserId == userId &&
            x.WarehouseId == warehouseId &&
            x.IsActive &&
            !x.IsDeleted,
            ct);

    public async Task<IReadOnlyList<Guid>> GetAssignedWarehouseIdsAsync(
        Guid userId,
        CancellationToken ct = default)
        => await _db
            .Where(x =>
                x.UserId == userId &&
                x.IsActive &&
                !x.IsDeleted)
            .Select(x => x.WarehouseId)
            .Distinct()
            .ToListAsync(ct);
}

// ================================================================
// STAFF PERMISSION REPOSITORY
// ================================================================

public class StaffPermissionRepository
    : Repository<StaffPermissionGrant>, IStaffPermissionRepository
{
    public StaffPermissionRepository(ApplicationDbContext ctx)
        : base(ctx)
    {
    }

    // ------------------------------------------------------------
    // GET ACTIVE PERMISSIONS BY USER
    // Chỉ lấy quyền đang hoạt động và chưa bị xóa mềm.
    // ------------------------------------------------------------

    public async Task<IReadOnlyList<StaffPermissionGrant>> GetByUserAsync(
        Guid userId,
        CancellationToken ct = default)
        => await _db
            .Include(x => x.Warehouse)
            .Where(x =>
                x.UserId == userId &&
                x.IsActive &&
                !x.IsDeleted)
            .ToListAsync(ct);

    // ------------------------------------------------------------
    // FIND EXISTING PERMISSION
    // Tìm cả quyền đang hoạt động, bị vô hiệu hóa hoặc xóa mềm.
    //
    // IgnoreQueryFilters() giúp tìm thấy bản ghi ngay cả khi
    // DbContext cấu hình global query filter.
    //
    // Dùng khi cấp quyền để tránh INSERT bản ghi trùng.
    // ------------------------------------------------------------

    public async Task<StaffPermissionGrant?> FindExistingAsync(
        Guid userId,
        StaffPermission permission,
        Guid? warehouseId,
        CancellationToken ct = default)
        => await _db
            .IgnoreQueryFilters()
            .Include(x => x.Warehouse)
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.Permission == permission &&
                x.WarehouseId == warehouseId,
                ct);

    // ------------------------------------------------------------
    // CHECK STAFF PERMISSION
    //
    // WarehouseId = null trong bản ghi quyền:
    // Có quyền trên tất cả kho.
    //
    // WarehouseId cụ thể:
    // Chỉ có quyền trong kho được cấp.
    // ------------------------------------------------------------

    public async Task<bool> HasPermissionAsync(
        Guid userId,
        StaffPermission permission,
        Guid? warehouseId = null,
        CancellationToken ct = default)
    {
        return await _db.AnyAsync(x =>
            x.UserId == userId &&
            x.Permission == permission &&
            x.IsActive &&
            !x.IsDeleted &&
            (
                x.WarehouseId == null ||
                (
                    warehouseId.HasValue &&
                    x.WarehouseId == warehouseId.Value
                )
            ),
            ct);
    }

    // ------------------------------------------------------------
    // GET ACTIVE PERMISSIONS BY USER AND PERMISSION TYPE
    // ------------------------------------------------------------

    public async Task<IReadOnlyList<StaffPermissionGrant>>
        GetByUserAndPermissionAsync(
            Guid userId,
            StaffPermission permission,
            CancellationToken ct = default)
        => await _db
            .Include(x => x.Warehouse)
            .Where(x =>
                x.UserId == userId &&
                x.Permission == permission &&
                x.IsActive &&
                !x.IsDeleted)
            .ToListAsync(ct);
}