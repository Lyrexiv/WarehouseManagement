namespace WarehouseManagement.Domain.Enums;

public enum GoodsReceiptStatus { Draft = 1, Approved = 2, Cancelled = 3 }
public enum GoodsIssueStatus   { Draft = 1, Approved = 2, Cancelled = 3 }
public enum StockAdjustmentType { Increase = 1, Decrease = 2 }
public enum InventoryAuditStatus { InProgress = 1, Completed = 2, Cancelled = 3 }
public enum MovementType { Receipt = 1, Issue = 2, Adjustment = 3, AuditAdjustment = 4 }
