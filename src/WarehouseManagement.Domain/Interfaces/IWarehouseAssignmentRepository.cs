using WarehouseManagement.Domain.Entities;

namespace WarehouseManagement.Domain.Interfaces;

// ================================================================
// WAREHOUSE ASSIGNMENT REPOSITORY
// ================================================================
public interface IWarehouseAssignmentRepository : IRepository<WarehouseAssignment>
{
    /// <summary>Lay danh sach kho duoc phan cong cho user</summary>
    Task<IReadOnlyList<WarehouseAssignment>> GetByUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Lay danh sach user duoc phan cong quan ly kho</summary>
    Task<IReadOnlyList<WarehouseAssignment>> GetByWarehouseAsync(Guid warehouseId, CancellationToken ct = default);

    /// <summary>Kiem tra user co duoc phan cong kho nay khong</summary>
    Task<bool> IsAssignedAsync(Guid userId, Guid warehouseId, CancellationToken ct = default);

    /// <summary>Lay danh sach WarehouseId duoc phan cong cho user</summary>
    Task<IReadOnlyList<Guid>> GetAssignedWarehouseIdsAsync(Guid userId, CancellationToken ct = default);
}

// ================================================================
// STAFF PERMISSION REPOSITORY
// ================================================================
public interface IStaffPermissionRepository : IRepository<StaffPermissionGrant>
{
    /// <summary>Lay tat ca quyen cua mot user</summary>
    Task<IReadOnlyList<StaffPermissionGrant>> GetByUserAsync(
        Guid userId,
        CancellationToken ct = default);

    /// <summary>Kiem tra user co quyen nay khong (tuy chon loc theo kho)</summary>
    Task<bool> HasPermissionAsync(
        Guid userId,
        StaffPermission permission,
        Guid? warehouseId = null,
        CancellationToken ct = default);

    /// <summary>Lay danh sach quyen + kho tuong ung cua user</summary>
    Task<IReadOnlyList<StaffPermissionGrant>> GetByUserAndPermissionAsync(
        Guid userId,
        StaffPermission permission,
        CancellationToken ct = default);

    /// <summary>
    /// Tim quyen da ton tai, bao gom ca quyen inactive hoac soft deleted.
    /// Dung de tranh tao ban ghi trung khoa.
    /// </summary>
    Task<StaffPermissionGrant?> FindExistingAsync(
        Guid userId,
        StaffPermission permission,
        Guid? warehouseId,
        CancellationToken ct = default);
}