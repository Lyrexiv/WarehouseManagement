using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.Enums;
namespace WarehouseManagement.Domain.Entities;

public class GoodsReceipt : BaseEntity
{
    public string ReceiptNumber { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public Guid SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;
    public DateTime ImportDate { get; set; }
    public decimal TotalAmount { get; set; }
    public GoodsReceiptStatus Status { get; set; } = GoodsReceiptStatus.Draft;
    public string? Note { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public ICollection<GoodsReceiptDetail> Details { get; set; } = new List<GoodsReceiptDetail>();

    public void CalculateTotalAmount() =>
        TotalAmount = Details.Sum(d => d.Quantity * d.UnitPrice);
}

public class GoodsReceiptDetail : BaseEntity
{
    public Guid GoodsReceiptId { get; set; }
    public GoodsReceipt GoodsReceipt { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
    public string? Note { get; set; }
}
