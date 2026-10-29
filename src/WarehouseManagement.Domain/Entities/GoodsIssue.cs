using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.Enums;
namespace WarehouseManagement.Domain.Entities;

public class GoodsIssue : BaseEntity
{
    public string IssueNumber { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public DateTime ExportDate { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public string? ReceiverDepartment { get; set; }
    public string? Purpose { get; set; }
    public GoodsIssueStatus Status { get; set; } = GoodsIssueStatus.Draft;
    public string? Note { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public ICollection<GoodsIssueDetail> Details { get; set; } = new List<GoodsIssueDetail>();
}

public class GoodsIssueDetail : BaseEntity
{
    public Guid GoodsIssueId { get; set; }
    public GoodsIssue GoodsIssue { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
    public string? Note { get; set; }
}
