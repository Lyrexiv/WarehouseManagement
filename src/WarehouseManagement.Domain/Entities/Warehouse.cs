using WarehouseManagement.Domain.Common;
namespace WarehouseManagement.Domain.Entities;

public class Warehouse : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Country { get; set; }
    public decimal Capacity { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Inventory> Inventories { get; set; } = new List<Inventory>();
    public ICollection<GoodsReceipt> GoodsReceipts { get; set; } = new List<GoodsReceipt>();
    public ICollection<GoodsIssue> GoodsIssues { get; set; } = new List<GoodsIssue>();
    public ICollection<InventoryAudit> InventoryAudits { get; set; } = new List<InventoryAudit>();
}
