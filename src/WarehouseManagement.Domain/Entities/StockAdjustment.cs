using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.Enums;
namespace WarehouseManagement.Domain.Entities;

public class StockAdjustment : BaseEntity
{
    public string AdjustmentNumber { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int QuantityBefore { get; set; }
    public int QuantityAfter { get; set; }
    public int AdjustmentQuantity => QuantityAfter - QuantityBefore;
    public StockAdjustmentType AdjustmentType { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Note { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
}

public class InventoryAudit : BaseEntity
{
    public string AuditNumber { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public DateTime AuditDate { get; set; }
    public InventoryAuditStatus Status { get; set; } = InventoryAuditStatus.InProgress;
    public string? Note { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid? CompletedByUserId { get; set; }
    public ICollection<InventoryAuditDetail> Details { get; set; } = new List<InventoryAuditDetail>();
}

public class InventoryAuditDetail : BaseEntity
{
    public Guid InventoryAuditId { get; set; }
    public InventoryAudit InventoryAudit { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int SystemQuantity { get; set; }
    public int ActualQuantity { get; set; }
    public int Discrepancy => ActualQuantity - SystemQuantity;
    public string? Note { get; set; }
}
