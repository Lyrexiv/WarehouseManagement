using WarehouseManagement.Domain.Common;

namespace WarehouseManagement.Domain.Entities;

// ================================================================
// WAREHOUSE ASSIGNMENT — Manager duoc phan cong quan ly kho nao
// ================================================================

/// <summary>
/// Bang phan cong kho cho Manager.
/// Admin toan quyen, khong can phan cong.
/// Manager chi thay/thao tac kho duoc phan cong.
/// </summary>
public class WarehouseAssignment : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    public bool IsActive { get; set; } = true;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public Guid AssignedByUserId { get; set; }
    public string? Note { get; set; }
}

// ================================================================
// STAFF PERMISSION — Staff duoc cap quyen nghiep vu gi
// ================================================================

public enum StaffPermission
{
    // Phieu nhap
    ViewGoodsReceipts = 10,
    CreateGoodsReceipt = 11,
    ApproveGoodsReceipt = 12,
    CancelGoodsReceipt = 13,

    // Phieu xuat
    ViewGoodsIssues = 20,
    CreateGoodsIssue = 21,
    ApproveGoodsIssue = 22,
    CancelGoodsIssue = 23,

    // Ton kho
    ViewInventory = 30,
    AdjustStock = 31,

    // Kiem ke
    ViewAudit = 40,
    CreateAudit = 41,
    CompleteAudit = 42,

    // Bao cao
    ViewReports = 50,

    // San pham / danh muc (xem thoi)
    ViewProducts = 60,
    ViewCategories = 61,
    ViewSuppliers = 62,
}

/// <summary>
/// Quyen nghiep vu cua Staff.
/// Moi Staff co the duoc cap nhieu quyen khac nhau.
/// Co the gioi han theo kho (WarehouseId null = tat ca kho co quyen).
/// </summary>
public class StaffPermissionGrant : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public StaffPermission Permission { get; set; }

    /// <summary>null = ap dung cho moi kho. Co gia tri = chi kho nay.</summary>
    public Guid? WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    public Guid GrantedByUserId { get; set; }
}