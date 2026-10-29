using WarehouseManagement.Domain.Common;
using WarehouseManagement.Domain.Enums;
namespace WarehouseManagement.Domain.Entities;

public class Inventory : BaseEntity
{
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; } = 0;
    public int ReservedQuantity { get; set; } = 0;
    public int AvailableQuantity => Quantity - ReservedQuantity;
    public bool IsLowStock => Product != null && Quantity <= Product.MinimumStock;

    public void AddStock(int qty)
    {
        if (qty <= 0) throw new ArgumentException("Quantity must be positive");
        Quantity += qty;
        UpdatedAt = DateTime.UtcNow;
    }
    public void RemoveStock(int qty)
    {
        if (qty <= 0) throw new ArgumentException("Quantity must be positive");
        if (AvailableQuantity < qty) throw new InvalidOperationException($"Insufficient stock. Available: {AvailableQuantity}, Requested: {qty}");
        Quantity -= qty;
        UpdatedAt = DateTime.UtcNow;
    }
    public void AdjustStock(int newQty)
    {
        if (newQty < 0) throw new ArgumentException("Quantity cannot be negative");
        Quantity = newQty;
        UpdatedAt = DateTime.UtcNow;
    }
}

public class StockMovement : BaseEntity
{
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public MovementType MovementType { get; set; }
    public int Quantity { get; set; }
    public int QuantityBefore { get; set; }
    public int QuantityAfter { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Note { get; set; }
    public Guid? CreatedByUserId { get; set; }
}
